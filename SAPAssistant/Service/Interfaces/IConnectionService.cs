using SAPAssistant.Models;
using SAPAssistant.Exceptions;

namespace SAPAssistant.Service.Interfaces
{
    public interface IConnectionService
    {
        Task<ResultMessage<List<ConnectionDTO>>> GetConnectionsAsync();
        Task<OperationResult<ConnectionDTO>> GetConnectionByIdAsync(string connectionId);
        Task<OperationResult> UpdateConnectionAsync(ConnectionDTO connection);
        Task<OperationResult> CreateConnectionAsync(ConnectionDTO connection);
        Task<OperationResult> ValidateConnectionAsync(string connectionId);
    }
}

