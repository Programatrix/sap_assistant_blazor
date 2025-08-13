using Microsoft.AspNetCore.Http;
using SAPAssistant.Models;
using System.Text.Json;

namespace SAPAssistant.Service
{
    public class SessionContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SessionContextService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
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
            Response?.Cookies.Append(key, value, BuildOptions(persistent));
            return ValueTask.CompletedTask;
        }

        private ValueTask DeleteCookie(string key)
        {
            Response?.Cookies.Delete(key);
            return ValueTask.CompletedTask;
        }

        // ----- Autenticación -----
        public Task<string?> GetUserIdAsync() => Task.FromResult(GetCookie("username"));

        public ValueTask SetUserIdAsync(string username, bool persistent = false)
            => SetCookie("username", username, persistent);

        public ValueTask DeleteUserIdAsync() => DeleteCookie("username");

        public Task<string?> GetTokenAsync() => Task.FromResult(GetCookie("token"));

        public ValueTask SetTokenAsync(string token, bool persistent = false)
            => SetCookie("token", token, persistent);

        public ValueTask DeleteTokenAsync() => DeleteCookie("token");

        public Task<string?> GetRemoteIpAsync()
        {
            var value = GetCookie("remote_url");
            return Task.FromResult(value?.TrimEnd('/'));
        }

        public ValueTask SetRemoteIpAsync(string remoteUrl, bool persistent = false)
            => SetCookie("remote_url", remoteUrl, persistent);

        public ValueTask DeleteRemoteIpAsync() => DeleteCookie("remote_url");

        // ----- Conexión activa -----
        public Task<string?> GetActiveConnectionIdAsync()
            => Task.FromResult(GetCookie("active_connection_id"));

        public ValueTask SetActiveConnectionIdAsync(string connectionId)
            => SetCookie("active_connection_id", connectionId);

        public ValueTask DeleteActiveConnectionIdAsync()
            => DeleteCookie("active_connection_id");

        public Task<string?> GetDatabaseTypeAsync()
            => Task.FromResult(GetCookie("active_db_type"));

        public ValueTask SetDatabaseTypeAsync(string dbType)
            => SetCookie("active_db_type", dbType);

        public ValueTask DeleteDatabaseTypeAsync()
            => DeleteCookie("active_db_type");

        // ----- Edición de conexión -----
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

