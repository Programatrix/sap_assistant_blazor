using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;

namespace SAPAssistant.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    private readonly NavigationManager _navigationManager;

    [ObservableProperty]
    private string userName = "Usuario";

    [ObservableProperty]
    private string todayDate = DateTime.Now.ToString("dddd, dd MMMM yyyy");

    [ObservableProperty]
    private string prompt = string.Empty;

    public HomeViewModel(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }

    public async Task HandleQuickPrompt()
    {
        if (string.IsNullOrWhiteSpace(Prompt))
        {
            return;
        }

        await Task.Delay(500);
        _navigationManager.NavigateTo($"/assistant?prompt={Uri.EscapeDataString(Prompt)}");
    }
}

