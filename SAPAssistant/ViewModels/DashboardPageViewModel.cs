using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SAPAssistant.Service;

namespace SAPAssistant.ViewModels;

public partial class DashboardPageViewModel : BaseViewModel
{
    public DashboardService DashboardService { get; }

    [ObservableProperty]
    private bool isWizardOpen;

    public DashboardPageViewModel(DashboardService dashboardService)
    {
        DashboardService = dashboardService;
    }

    public void OpenWizard() => IsWizardOpen = true;

    public void CloseWizard() => IsWizardOpen = false;

    public void EditDashboard()
    {
        // Modo edición del dashboard se implementará en el futuro
    }

    public async Task RefreshAll()
    {
        await DashboardService.RefreshAllKPIs();
    }
}
