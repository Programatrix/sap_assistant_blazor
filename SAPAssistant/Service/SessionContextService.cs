using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using SAPAssistant.Models;

namespace SAPAssistant.Service
{
    public class SessionContextService
    {
        private readonly ProtectedSessionStorage _sessionStorage;

        public SessionContextService(ProtectedSessionStorage sessionStorage)
        {
            _sessionStorage = sessionStorage;
        }

        // ----- Autenticación -----
        public async Task<string?> GetUserIdAsync()
        {
            var result = await _sessionStorage.GetAsync<string>("username");
            return result.Success ? result.Value : null;
        }

        public ValueTask SetUserIdAsync(string username)
            => _sessionStorage.SetAsync("username", username);

        public ValueTask DeleteUserIdAsync()
            => _sessionStorage.DeleteAsync("username");

        public async Task<string?> GetTokenAsync()
        {
            var result = await _sessionStorage.GetAsync<string>("token");
            return result.Success ? result.Value : null;
        }

        public ValueTask SetTokenAsync(string token)
            => _sessionStorage.SetAsync("token", token);

        public ValueTask DeleteTokenAsync()
            => _sessionStorage.DeleteAsync("token");

        public async Task<string?> GetRemoteIpAsync()
        {
            var result = await _sessionStorage.GetAsync<string>("remote_url");
            return result.Success ? result.Value?.TrimEnd('/') : null;
        }

        public ValueTask SetRemoteIpAsync(string remoteUrl)
            => _sessionStorage.SetAsync("remote_url", remoteUrl);

        public ValueTask DeleteRemoteIpAsync()
            => _sessionStorage.DeleteAsync("remote_url");

        // ----- Conexión activa -----
        public async Task<string?> GetActiveConnectionIdAsync()
        {
            var result = await _sessionStorage.GetAsync<string>("active_connection_id");
            return result.Success ? result.Value : null;
        }

        public ValueTask SetActiveConnectionIdAsync(string connectionId)
            => _sessionStorage.SetAsync("active_connection_id", connectionId);

        public ValueTask DeleteActiveConnectionIdAsync()
            => _sessionStorage.DeleteAsync("active_connection_id");

        public async Task<string?> GetDatabaseTypeAsync()
        {
            var result = await _sessionStorage.GetAsync<string>("active_db_type");
            return result.Success ? result.Value : null;
        }

        public ValueTask SetDatabaseTypeAsync(string dbType)
            => _sessionStorage.SetAsync("active_db_type", dbType);

        public ValueTask DeleteDatabaseTypeAsync()
            => _sessionStorage.DeleteAsync("active_db_type");

        // ----- Edición de conexión -----
        public async Task<ConnectionDTO?> GetConnectionToEditAsync()
        {
            var result = await _sessionStorage.GetAsync<ConnectionDTO>("connection_to_edit");
            return result.Success ? result.Value : null;
        }

        public ValueTask SetConnectionToEditAsync(ConnectionDTO connection)
            => _sessionStorage.SetAsync("connection_to_edit", connection);

        public ValueTask DeleteConnectionToEditAsync()
            => _sessionStorage.DeleteAsync("connection_to_edit");
    }
}

