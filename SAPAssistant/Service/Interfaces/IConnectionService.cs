using SAPAssistant.Models;
using SAPAssistant.Exceptions;

namespace SAPAssistant.Service.Interfaces
{
    public interface IConnectionService
    {
        Task<ResultMessage<List<ConnectionDTO>>> GetConnectionsAsync();
        Task<ConnectionDTO?> GetConnectionByIdAsync(string connectionId);
        Task<bool> UpdateConnectionAsync(ConnectionDTO connection);
        Task<bool> CreateConnectionAsync(ConnectionDTO connection);
        Task<bool> ValidateConnectionAsync(string connectionId);
    }
}

