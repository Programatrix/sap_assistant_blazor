using Microsoft.AspNetCore.Components;
using CommunityToolkit.Mvvm.ComponentModel;
using SAPAssistant.Service;
using Microsoft.Extensions.Logging;

namespace SAPAssistant.ViewModels;

public partial class DashboardWizardViewModel : BaseViewModel
{
    private readonly DashboardService _dashboardService;

    [ObservableProperty]
    private NewKpiModel newKpiModel = new();

    public EventCallback OnFinished { get; set; }

    public DashboardWizardViewModel(
        DashboardService dashboardService,
        NotificationService notificationService,
        ILogger<DashboardWizardViewModel> logger) : base(notificationService, logger)
    {
        _dashboardService = dashboardService;
    }

    public async Task CreateKPI()
    {
        await _dashboardService.CreateNewKPI(NewKpiModel.Prompt, NewKpiModel.ChartType);
        if (OnFinished.HasDelegate)
        {
            await OnFinished.InvokeAsync();
        }
    }

    public async Task Cancel()
    {
        if (OnFinished.HasDelegate)
        {
            await OnFinished.InvokeAsync();
        }
    }
}

public class NewKpiModel
{
    public string Prompt { get; set; } = "";
    public string ChartType { get; set; } = "bar";
}
