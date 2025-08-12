using SAPAssistant.Models;
using SAPAssistant.Models.Chat;
using SAPAssistant.Exceptions;

namespace SAPAssistant.Service.Interfaces
{
    public interface IChatHistoryService
    {
        Task<ServiceResult<List<ChatSession>>> GetChatHistoryAsync();
        Task<ServiceResult<ChatSession>> GetChatSessionAsync(string chatId);
        Task<ServiceResult> SaveChatSessionAsync(ChatSession session, List<MessageBase> mensajes);
        Task<ServiceResult> DeleteChatSessionAsync(string chatId);
        Task<ServiceResult<ChatSession>> GetLastChatSessionAsync();
    }
}
