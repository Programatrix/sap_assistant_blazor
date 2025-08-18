using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using SAPAssistant.Models;
using System.Text.Json;

namespace SAPAssistant.Service
{
    public class SessionContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ProtectedSessionStorage _sessionStorage;

        public SessionContextService(IHttpContextAccessor httpContextAccessor, ProtectedSessionStorage sessionStorage)
        {
            _httpContextAccessor = httpContextAccessor;
            _sessionStorage = sessionStorage;
        }

        private HttpRequest? Request => _httpContextAccessor.HttpContext?.Request;
        private HttpResponse? Response => _httpContextAccessor.HttpContext?.Response;

        private CookieOptions BuildOptions(bool persistent = false)
        {
            var options = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            };
            if (persistent)
            {
                options.Expires = DateTimeOffset.UtcNow.AddDays(30);
            }
            return options;
        }

        private string? GetCookie(string key)
            => Request?.Cookies.TryGetValue(key, out var value) == true ? value : null;

        private ValueTask SetCookie(string key, string value, bool persistent = false)
        {
            if (Response is null || Response.HasStarted)
            {
                return ValueTask.CompletedTask;
            }

            Response.Cookies.Append(key, value, BuildOptions(persistent));
            return ValueTask.CompletedTask;
        }

        private ValueTask DeleteCookie(string key)
        {
            Response?.Cookies.Delete(key);
            return ValueTask.CompletedTask;
        }

        // ----- Autenticación (cookies igual que antes) -----
        public ValueTask SetUserIdAsync(string userId, bool persistent = false)
            => SetCookie("username", userId, persistent);

        public ValueTask DeleteUserIdAsync() => DeleteCookie("username");

        public ValueTask SetTokenAsync(string token, bool persistent = false)
            => SetCookie("token", token, persistent);

        public ValueTask DeleteTokenAsync() => DeleteCookie("token");

        public Task<string?> GetTokenAsync()
            => Task.FromResult(GetCookie("token"));

        public Task<string?> GetRemoteIpAsync()
        {
            var value = GetCookie("remote_url");
            return Task.FromResult(value?.TrimEnd('/'));
        }

        public ValueTask SetRemoteIpAsync(string remoteUrl, bool persistent = false)
            => SetCookie("remote_url", remoteUrl, persistent);

        public ValueTask DeleteRemoteIpAsync() => DeleteCookie("remote_url");


        // ----- Conexión activa (USANDO ProtectedSessionStorage) -----
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


        // ----- Edición de conexión (cookies, igual que antes) -----
        public async Task<ConnectionDTO?> GetConnectionToEditAsync()
        {
            var json = GetCookie("connection_to_edit");
            return json is null ? null : JsonSerializer.Deserialize<ConnectionDTO>(json);
        }

        public ValueTask SetConnectionToEditAsync(ConnectionDTO connection)
            => SetCookie("connection_to_edit", JsonSerializer.Serialize(connection));

        public ValueTask DeleteConnectionToEditAsync()
            => DeleteCookie("connection_to_edit");
    }
}
