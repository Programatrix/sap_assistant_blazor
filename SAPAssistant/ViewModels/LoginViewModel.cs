using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;
using SAPAssistant.Models;
using SAPAssistant.Service;

namespace SAPAssistant.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly AuthService _authService;
    private readonly NavigationManager _navigation;
    private readonly NotificationService _notificationService;

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
        NotificationService notificationService)
    {
        _authService = authService;
        _navigation = navigation;
        _notificationService = notificationService;
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
                _navigation.NavigateTo("/");
            }
            else
            {
                _notificationService.Notify(result);
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

