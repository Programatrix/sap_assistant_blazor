using SAPAssistant.Models;
using SAPAssistant.Exceptions;

namespace SAPAssistant.Service.Interfaces
{
    public interface IConnectionService
    {
        Task<ServiceResult<List<ConnectionDTO>>> GetConnectionsAsync();
        Task<ServiceResult<ConnectionDTO>> GetConnectionByIdAsync(string connectionId);
        Task<ServiceResult> UpdateConnectionAsync(ConnectionDTO connection);
        Task<ServiceResult> CreateConnectionAsync(ConnectionDTO connection);
        Task<ServiceResult> ValidateConnectionAsync(string connectionId);
    }
}

