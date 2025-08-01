using SAPAssistant.Models;

namespace SAPAssistant.Service.Interfaces
{
    public interface IUserDashboardService
    {
        Task AddKpiAsync(DashboardCardModel kpi);
    }
}

