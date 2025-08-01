using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using SAPAssistant.Models;
using SAPAssistant.Service;

namespace SAPAssistant.ViewModels;

public partial class ConnectionSettingsViewModel : BaseViewModel
{
    private readonly ProtectedSessionStorage _sessionStorage;
    private readonly ConnectionService _connectionService;
    private readonly NavigationManager _navigation;

    [ObservableProperty]
    private ConnectionDTO connectionData = new();

    public bool IsEditMode => !string.IsNullOrWhiteSpace(ConnectionData.ConnectionId);

    public ConnectionSettingsViewModel(
        ProtectedSessionStorage sessionStorage,
        ConnectionService connectionService,
        NavigationManager navigation)
    {
        _sessionStorage = sessionStorage;
        _connectionService = connectionService;
        _navigation = navigation;
    }

    public async Task InitializeAsync()
    {
        var result = await _sessionStorage.GetAsync<ConnectionDTO>("connection_to_edit");
        if (result.Success && result.Value is not null)
        {
            ConnectionData = result.Value;
            await _sessionStorage.DeleteAsync("connection_to_edit");
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
            Console.WriteLine("❌ Error al guardar la conexión.");
        }
    }
}

