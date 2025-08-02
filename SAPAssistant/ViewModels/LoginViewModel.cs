using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;
using SAPAssistant.Models;
using SAPAssistant.Exceptions;
using SAPAssistant.Service;
using Microsoft.Extensions.Logging;

namespace SAPAssistant.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly AuthService _authService;
    private readonly NavigationManager _navigation;
    private readonly NotificationService _notificationService;
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
        NotificationService notificationService,
        StateContainer stateContainer,
        ILogger<LoginViewModel> logger) : base(notificationService, logger)
    {
        _authService = authService;
        _navigation = navigation;
        _notificationService = notificationService;
        _stateContainer = stateContainer;
    }

    public async Task HandleLogin()
    {
        IsLoading = true;

        UserNameError = string.IsNullOrWhiteSpace(LoginModel.Username);
        PasswordError = string.IsNullOrWhiteSpace(LoginModel.Password);

        if (!UserNameError && !PasswordError)
        {
            var result = await _authService.LoginAsync(LoginModel);

            if (result.Success)
            {
                _stateContainer.AuthenticatedUser = result.Data;
                _navigation.NavigateTo("/");
            }
            else
            {
                var notify = new ResultMessage
                {
                    Success = result.Success,
                    Message = result.Message,
                    ErrorCode = result.ErrorCode,
                    Type = result.Type
                };
                _notificationService.Notify(notify);
                PasswordError = true;
            }
        }

        IsLoading = false;
    }

    public void TogglePasswordVisibility() => ShowPassword = !ShowPassword;

    public void ValidateFields()
    {
        UserNameError = string.IsNullOrWhiteSpace(LoginModel.Username);
        PasswordError = string.IsNullOrWhiteSpace(LoginModel.Password);
    }

    public string GetInputClass(bool hasError) => hasError ? "input input-error" : "input";
}

