using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;
using SAPAssistant.Models;
using SAPAssistant.Exceptions;
using SAPAssistant.Service;
using SAPAssistant.Service.Interfaces;

namespace SAPAssistant.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly AuthService _authService;
    private readonly NavigationManager _navigation;
    private readonly INotificationService _notificationService;
    private readonly StateContainer _stateContainer;

    [ObservableProperty]
    private LoginRequest loginModel = new();

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool userNameError;

    [ObservableProperty]
    private bool passwordError;

    [ObservableProperty]
    private bool showPassword;

    public bool CanSubmit =>
        !string.IsNullOrWhiteSpace(LoginModel.Username) &&
        !string.IsNullOrWhiteSpace(LoginModel.Password);

    public LoginViewModel(
        AuthService authService,
        NavigationManager navigation,
        INotificationService notificationService,
        StateContainer stateContainer) : base(notificationService)
    {
        _authService = authService;
        _navigation = navigation;
        _notificationService = notificationService;
        _stateContainer = stateContainer;
    }

    /// <summary>
    /// Maneja el proceso de login con validación visual y notificaciones
    /// </summary>
    public async Task HandleLogin()
    {
        IsLoading = true;

        // 1️⃣ Validación visual
        ValidateFields();
        if (UserNameError || PasswordError)
        {
            IsLoading = false;
            return; // No mostramos toasts, solo resaltamos campos
        }

        // 2️⃣ Llamada al servicio de autenticación
        var result = await _authService.LoginAsync(LoginModel);

        if (result.Success)
        {
            _stateContainer.AuthenticatedUser = result.Data;
            _navigation.NavigateTo("/");
        }
        else
        {
            // 3️⃣ Notificación de error de negocio / autenticación
            var notify = new ResultMessage
            {
                Success = result.Success,
                Message = result.Message,
                ErrorCode = result.ErrorCode,
                Type = result.Type
            };

            await _notificationService.Notify(notify);

            // Marcar solo la contraseña como errónea para feedback visual
            PasswordError = true;
        }

        IsLoading = false;
    }

    /// <summary>
    /// Alterna la visibilidad de la contraseña
    /// </summary>
    public void TogglePasswordVisibility() => ShowPassword = !ShowPassword;

    /// <summary>
    /// Valida que los campos requeridos no estén vacíos
    /// </summary>
    public void ValidateFields()
    {
        UserNameError = string.IsNullOrWhiteSpace(LoginModel.Username);
        PasswordError = string.IsNullOrWhiteSpace(LoginModel.Password);
    }

    /// <summary>
    /// Devuelve la clase CSS en función del estado del input
    /// </summary>
    public string GetInputClass(bool hasError) => hasError ? "input input-error" : "input";
}

