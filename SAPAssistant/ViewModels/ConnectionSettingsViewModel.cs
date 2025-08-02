using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;
using SAPAssistant.Models;
using SAPAssistant.Service;
using SAPAssistant.Service.Interfaces;

namespace SAPAssistant.ViewModels;

public partial class ConnectionSettingsViewModel : BaseViewModel
{
    private readonly SessionContextService _sessionContext;
    private readonly IConnectionService _connectionService;
    private readonly NavigationManager _navigation;
    private readonly NotificationService _notificationService;

    [ObservableProperty]
    private ConnectionDTO connectionData = new();

    public bool IsEditMode => !string.IsNullOrWhiteSpace(ConnectionData.ConnectionId);

    public ConnectionSettingsViewModel(
        SessionContextService sessionContext,
        IConnectionService connectionService,
        NavigationManager navigation,
        NotificationService notificationService)
    {
        _sessionContext = sessionContext;
        _connectionService = connectionService;
        _navigation = navigation;
        _notificationService = notificationService;
    }

    public async Task InitializeAsync()
    {
        var result = await _sessionContext.GetConnectionToEditAsync();
        if (result is not null)
        {
            ConnectionData = result;
            await _sessionContext.DeleteConnectionToEditAsync();
        }
    }

    public async Task HandleSave()
    {
        bool success;

        if (IsEditMode)
        {
            success = await _connectionService.UpdateConnectionAsync(ConnectionData);
        }
        else
        {
            success = await _connectionService.CreateConnectionAsync(ConnectionData);
        }

        if (success)
        {
            _navigation.NavigateTo("/");
        }
        else
        {
            _notificationService.NotifyError("❌ Error al guardar la conexión.");
        }
    }
}

