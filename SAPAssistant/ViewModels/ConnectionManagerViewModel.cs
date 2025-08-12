using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;
using SAPAssistant.Models;
using SAPAssistant.Service;
using SAPAssistant.Service.Interfaces;
using SAPAssistant.Exceptions;

namespace SAPAssistant.ViewModels;

public partial class ConnectionManagerViewModel : BaseViewModel
{
    private readonly IConnectionService _connectionService;
    private readonly SessionContextService _sessionContext;
    private readonly NavigationManager _navigation;
    private readonly StateContainer _stateContainer;

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

    public ConnectionManagerViewModel(
        IConnectionService connectionService,
        SessionContextService sessionContext,
        NavigationManager navigation,
        StateContainer stateContainer,
        INotificationService notificationService) : base(notificationService)
    {
        _connectionService = connectionService;
        _sessionContext = sessionContext;
        _navigation = navigation;
        _stateContainer = stateContainer;
    }

    public async Task LoadConnections()
    {
        var result = await _connectionService.GetConnectionsAsync();
        if (result.Success)
        {
            Connections = result.Data ?? new();
            var activeId = await _sessionContext.GetActiveConnectionIdAsync();
            foreach (var conn in Connections)
                conn.IsActive = conn.ConnectionId == activeId;
        }
        else
        {
            ErrorAlCargar = result.Message;
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
            var result = await _connectionService.ValidateConnectionAsync(connection.ConnectionId);
            if (!result.Success)
            {
                MostrarError = true;
                await NotificationService.Notify(result);
                await Task.Delay(3000);
                MostrarError = false;
                return false;
            }

            await _sessionContext.SetActiveConnectionIdAsync(connection.ConnectionId);
            await _sessionContext.SetDatabaseTypeAsync(connection.db_type);

            _stateContainer.ActiveConnection = connection;

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

        await _sessionContext.SetConnectionToEditAsync(conn);
        _navigation.NavigateTo("/connection/edit", esModal);
    }

    public void NuevaConexion(bool esModal)
    {
        if (IsActivating)
            return;

        _navigation.NavigateTo("/connection/edit", esModal);
    }
}
