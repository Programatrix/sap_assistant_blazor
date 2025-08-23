using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;
using SAPAssistant.Models;
using SAPAssistant.Service;
using SAPAssistant.Service.Interfaces;

namespace SAPAssistant.ViewModels;

public partial class ChatHistoryViewModel : BaseViewModel
{
    private readonly IChatHistoryService _chatService;
    private readonly NavigationManager _navigation;

    [ObservableProperty]
    private List<ChatSession> sessions = new();

    [ObservableProperty]
    private bool isLoading = false;

    // ✅ Nuevo: sabemos si ya cargamos el historial
    [ObservableProperty]
    private bool isLoaded = false;

    public ChatHistoryViewModel(
        IChatHistoryService chatService,
        NavigationManager navigation,
        INotificationService notificationService) : base(notificationService)
    {
        _chatService = chatService;
        _navigation = navigation;
    }

    public async Task LoadHistoryAsync(bool force = false)
    {
        // ✅ Guardas para evitar recargas innecesarias
        if (IsLoaded && !force) return;
        if (IsLoading) return;

        IsLoading = true;
        try
        {
            var result = await _chatService.GetChatHistoryAsync();
            if (result.Success && result.Data != null)
            {
                Sessions = result.Data;
                IsLoaded = true;
            }
            else
            {
                Sessions = new List<ChatSession>();
                IsLoaded = false;
                await NotificationService.Notify(result);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void StartNewChat()
    {
        _navigation.NavigateTo("/");
    }

    public void OpenChat(string chatId)
    {
        // ❌ Antes: forceLoad: true (forzaba recarga completa del sitio)
        // ✅ Ahora: navegación SPA sin recargar todo
        _navigation.NavigateTo($"/chat/{chatId}");
    }

    public async Task DeleteChat(string chatId)
    {
        var result = await _chatService.DeleteChatSessionAsync(chatId);
        if (!result.Success)
        {
            await NotificationService.Notify(result);
            return;
        }

        // ✅ Actualizamos en memoria sin reconsultar todo
        Sessions = Sessions.Where(s => s.Id != chatId).ToList();

        // Si borras todo, puedes marcarlo como no cargado si quieres volver a consultar la próxima vez
        if (Sessions.Count == 0) IsLoaded = false;
    }

    // (Opcional) Método para refrescar manualmente cuando tú quieras
    public Task RefreshAsync() => LoadHistoryAsync(force: true);
}
