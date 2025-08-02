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
    private readonly NotificationService _notificationService;

    [ObservableProperty]
    private List<ChatSession> sessions = new();

    [ObservableProperty]
    private bool isLoading = true;

    public ChatHistoryViewModel(IChatHistoryService chatService, NavigationManager navigation, NotificationService notificationService)
    {
        _chatService = chatService;
        _navigation = navigation;
        _notificationService = notificationService;
    }

    public async Task LoadHistoryAsync()
    {
        try
        {
            IsLoading = true;
            Sessions = await _chatService.GetChatHistoryAsync();
        }
        catch (Exception ex)
        {
            _notificationService.NotifyError($"❌ Error al cargar el historial: {ex.Message}");
            Sessions = new List<ChatSession>();
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
        _navigation.NavigateTo($"/chat/{chatId}", forceLoad: true);
    }

    public async Task DeleteChat(string chatId)
    {
        try
        {
            await _chatService.DeleteChatSessionAsync(chatId);
            await LoadHistoryAsync();
        }
        catch (Exception ex)
        {
            _notificationService.NotifyError($"❌ Error al eliminar el chat: {ex.Message}");
        }
    }
}

