using SAPAssistant.Models;
using SAPAssistant.Exceptions;

namespace SAPAssistant.Service.Interfaces
{
    public interface IUserDashboardService
    {
        Task<ServiceResult> AddKpiAsync(DashboardCardModel kpi);
    }
}

