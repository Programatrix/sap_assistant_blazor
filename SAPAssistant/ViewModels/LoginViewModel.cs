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

    //public bool CanSubmit =>
    //    !string.IsNullOrWhiteSpace(LoginModel.Username) &&
    //    !string.IsNullOrWhiteSpace(LoginModel.Password);

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
        if (IsLoading) return; // evita doble submit
        IsLoading = true;

        try
        {
            // 1) Validación visual
            ValidateFields();
            if (UserNameError || PasswordError)
                return; // no toasts; feedback inline

            // 2) Llamada al servicio (ya devuelve ResultMessage sin lanzar)
            var result = await _authService.LoginAsync(LoginModel);

            if (result.Success)
            {
                _stateContainer.AuthenticatedUser = result.Data;
                _navigation.NavigateTo("/chat");
                return;
            }

            // 3) Notificación de error (backend o FE_*)
            var msg = string.IsNullOrWhiteSpace(result.Message) ? "Ocurrió un error." : result.Message!;
            if (!string.IsNullOrWhiteSpace(result.TraceId))
                msg += $" (trace: {result.TraceId})";

            await _notificationService.Notify(new ResultMessage
            {
                Success = false,
                Message = msg,
                ErrorCode = result.ErrorCode,
                Type = result.Type
            });

            // feedback visual mínimo
            PasswordError = true;
        }
        catch (Exception ex)
        {
            // Último cinturón de seguridad: nada se escapa al circuito
            await _notificationService.NotifyError(ex.Message, "FE_RENDER_UNCAUGHT");            
        }
        finally
        {
            IsLoading = false;            
        }
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

