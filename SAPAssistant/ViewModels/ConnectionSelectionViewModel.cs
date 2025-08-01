using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using SAPAssistant.Service;

namespace SAPAssistant.ViewModels;

public partial class ConnectionSelectionViewModel : BaseViewModel
{
    private readonly ProtectedSessionStorage _sessionStorage;
    private readonly ConnectionService _connectionService;
    private readonly NavigationManager _navigation;

    [ObservableProperty]
    private bool hayConexionActiva;

    [ObservableProperty]
    private bool mostrarAviso;

    public ConnectionSelectionViewModel(
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
        var result = await _sessionStorage.GetAsync<string>("active_connection_id");
        if (result.Success && !string.IsNullOrEmpty(result.Value))
        {
            var isValid = await _connectionService.ValidateConnectionAsync(result.Value);
            if (isValid)
            {
                HayConexionActiva = true;
            }
            else
            {
                HayConexionActiva = false;
                MostrarAviso = true;
                await _sessionStorage.DeleteAsync("active_connection_id");
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

