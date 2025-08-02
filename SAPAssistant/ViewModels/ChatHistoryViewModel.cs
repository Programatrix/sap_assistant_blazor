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
    private bool isLoading = true;

    public ChatHistoryViewModel(
        IChatHistoryService chatService,
        NavigationManager navigation,
        INotificationService notificationService) : base(notificationService)
    {
        _chatService = chatService;
        _navigation = navigation;
    }

    public async Task LoadHistoryAsync()
    {
        IsLoading = true;
        var success = await ExecuteSafeAsync(async () =>
        {
            Sessions = await _chatService.GetChatHistoryAsync();
        }, "Error al cargar el historial");

        if (!success)
        {
            Sessions = new List<ChatSession>();
        }
        IsLoading = false;
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
        await ExecuteSafeAsync(async () =>
        {
            await _chatService.DeleteChatSessionAsync(chatId);
            await LoadHistoryAsync();
        }, "Error al eliminar el chat");
    }
}

