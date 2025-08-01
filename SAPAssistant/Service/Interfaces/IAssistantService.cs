using SAPAssistant.Models;

namespace SAPAssistant.Service.Interfaces
{
    public interface IAssistantService
    {
        Task<QueryResponse?> ConsultarAsync(string mensaje, string chatId);
        Task<QueryResponse?> ConsultarDemoAsync(string mensaje);
    }
}

