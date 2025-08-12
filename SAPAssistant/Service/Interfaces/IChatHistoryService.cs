using SAPAssistant.Models;
using SAPAssistant.Models.Chat;
using SAPAssistant.Exceptions;

namespace SAPAssistant.Service.Interfaces
{
    public interface IChatHistoryService
    {
        Task<List<ChatSession>> GetChatHistoryAsync();
        Task<ServiceResult<ChatSession>> GetChatSessionAsync(string chatId);
        Task SaveChatSessionAsync(ChatSession session, List<MessageBase> mensajes);
        Task DeleteChatSessionAsync(string chatId);
        Task<ChatSession?> GetLastChatSessionAsync();
    }
}
