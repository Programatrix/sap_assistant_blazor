using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;
using SAPAssistant.Service;
using SAPAssistant.Service.Interfaces;

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
        NavigationManager navigation)
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
            var isValid = await _connectionService.ValidateConnectionAsync(activeId);
            if (isValid)
            {
                HayConexionActiva = true;
            }
            else
            {
                HayConexionActiva = false;
                MostrarAviso = true;
                await _sessionContext.DeleteActiveConnectionIdAsync();
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

