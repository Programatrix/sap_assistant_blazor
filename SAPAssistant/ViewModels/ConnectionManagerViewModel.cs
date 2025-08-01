using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using SAPAssistant.Models;
using SAPAssistant.Service;
using SAPAssistant.Service.Interfaces;

namespace SAPAssistant.ViewModels;

public partial class ConnectionManagerViewModel : BaseViewModel
{
    private readonly IConnectionService _connectionService;
    private readonly ProtectedSessionStorage _sessionStorage;
    private readonly NavigationManager _navigation;

    [ObservableProperty]
    private List<ConnectionDTO> connections = new();

    [ObservableProperty]
    private bool mostrarMensaje;

    [ObservableProperty]
    private bool mostrarError;

    [ObservableProperty]
    private bool isActivating;

    [ObservableProperty]
    private string? activatingConnectionId;

    [ObservableProperty]
    private string? errorAlCargar;

    public ConnectionManagerViewModel(IConnectionService connectionService, ProtectedSessionStorage sessionStorage, NavigationManager navigation)
    {
        _connectionService = connectionService;
        _sessionStorage = sessionStorage;
        _navigation = navigation;
    }

    public async Task LoadConnections()
    {
        var result = await _connectionService.GetConnectionsAsync();
        if (result.Success)
        {
            Connections = result.Data ?? new();
            var activeResult = await _sessionStorage.GetAsync<string>("active_connection_id");
            var activeId = activeResult.Success ? activeResult.Value : null;
            foreach (var conn in Connections)
                conn.IsActive = conn.ConnectionId == activeId;
        }
        else
        {
            ErrorAlCargar = $"❌ {result.Message} (Código: {result.ErrorCode})";
        }
    }

    public async Task RecargarConexiones()
    {
        ErrorAlCargar = null;
        await LoadConnections();
    }

    public async Task<bool> SetActive(ConnectionDTO connection)
    {
        if (IsActivating)
            return false;

        IsActivating = true;
        ActivatingConnectionId = connection.ConnectionId;
        MostrarMensaje = false;
        MostrarError = false;

        try
        {
            var isValid = await _connectionService.ValidateConnectionAsync(connection.ConnectionId);
            if (!isValid)
            {
                MostrarError = true;
                await Task.Delay(3000);
                MostrarError = false;
                return false;
            }

            await _sessionStorage.SetAsync("active_connection_id", connection.ConnectionId);
            await _sessionStorage.SetAsync("active_db_type", connection.db_type);

            foreach (var c in Connections)
                c.IsActive = c.ConnectionId == connection.ConnectionId;

            MostrarMensaje = true;
            await Task.Delay(3000);
            MostrarMensaje = false;
            return true;
        }
        finally
        {
            IsActivating = false;
            ActivatingConnectionId = null;
        }
    }

    public async Task EditarConexion(ConnectionDTO conn, bool esModal)
    {
        if (IsActivating)
            return;

        await _sessionStorage.SetAsync("connection_to_edit", conn);
        _navigation.NavigateTo("/connection/edit", esModal);
    }

    public void NuevaConexion(bool esModal)
    {
        if (IsActivating)
            return;

        _navigation.NavigateTo("/connection/edit", esModal);
    }
}
