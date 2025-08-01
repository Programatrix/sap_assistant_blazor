using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace SAPAssistant.Service
{
    public class SessionContextService
    {
        private readonly ProtectedSessionStorage _sessionStorage;

        public SessionContextService(ProtectedSessionStorage sessionStorage)
        {
            _sessionStorage = sessionStorage;
        }

        public async Task<string?> GetUserIdAsync()
        {
            var result = await _sessionStorage.GetAsync<string>("username");
            return result.Success ? result.Value : null;
        }

        public async Task<string?> GetTokenAsync()
        {
            var result = await _sessionStorage.GetAsync<string>("token");
            return result.Success ? result.Value : null;
        }

        public async Task<string?> GetRemoteIpAsync()
        {
            var result = await _sessionStorage.GetAsync<string>("remote_url");
            return result.Success ? result.Value?.TrimEnd('/') : null;
        }

        public async Task<string?> GetActiveConnectionIdAsync()
        {
            var result = await _sessionStorage.GetAsync<string>("active_connection_id");
            return result.Success ? result.Value : null;
        }

        public async Task<string?> GetDatabaseTypeAsync()
        {
            var result = await _sessionStorage.GetAsync<string>("active_db_type");
            return result.Success ? result.Value : null;
        }
    }
}

