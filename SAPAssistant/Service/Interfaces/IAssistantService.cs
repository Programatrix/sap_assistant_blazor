using SAPAssistant.Models;
using SAPAssistant.Exceptions;

namespace SAPAssistant.Service.Interfaces
{
    public interface IAssistantService
    {
        Task<ServiceResult<QueryResponse>> ConsultarAsync(string mensaje, string chatId);
        Task<ServiceResult<QueryResponse>> ConsultarDemoAsync(string mensaje);
        Task<ServiceResult<string>> StartQueryAsync(string mensaje, string chatId);
    }
}

