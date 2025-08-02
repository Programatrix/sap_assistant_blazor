using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;
using SAPAssistant.Service;
using SAPAssistant.Service.Interfaces;
using SAPAssistant.Exceptions;

namespace SAPAssistant.ViewModels;

public partial class ConnectionSelectionViewModel : BaseViewModel
{
    private readonly SessionContextService _sessionContext;
    private readonly IConnectionService _connectionService;
    private readonly NavigationManager _navigation;

    [ObservableProperty]
    private bool hayConexionActiva;

    [ObservableProperty]
    private bool mostrarAviso;

    public ConnectionSelectionViewModel(
        SessionContextService sessionContext,
        IConnectionService connectionService,
        NavigationManager navigation,
        INotificationService notificationService) : base(notificationService)
    {
        _sessionContext = sessionContext;
        _connectionService = connectionService;
        _navigation = navigation;
    }

    public async Task InitializeAsync()
    {
        var activeId = await _sessionContext.GetActiveConnectionIdAsync();
        if (!string.IsNullOrEmpty(activeId))
        {
            var result = await _connectionService.ValidateConnectionAsync(activeId);
            if (result.Success)
            {
                HayConexionActiva = true;
            }
            else
            {
                HayConexionActiva = false;
                MostrarAviso = true;
                await _sessionContext.DeleteActiveConnectionIdAsync();
                var notify = new ResultMessage
                {
                    Success = result.Success,
                    Message = result.Message,
                    ErrorCode = result.ErrorCode,
                    Type = NotificationType.Error
                };
                NotificationService.Notify(notify);
            }
        }
    }

    public Task HandleConnectionActivated(bool activo)
    {
        HayConexionActiva = activo;
        MostrarAviso = false;
        return Task.CompletedTask;
    }

    public void VolverHome() => _navigation.NavigateTo("/");
}

