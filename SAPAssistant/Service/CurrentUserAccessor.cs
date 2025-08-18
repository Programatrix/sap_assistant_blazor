// Services/CurrentUserAccessor.cs
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;

namespace SAPAssistant.Service
{
    public class CurrentUserAccessor
    {
        private readonly AuthenticationStateProvider? _auth;
        private readonly IHttpContextAccessor _http;

        public CurrentUserAccessor(IHttpContextAccessor http, AuthenticationStateProvider? auth = null)
        {
            _http = http;
            _auth = auth; // opcional
        }

        private async Task<ClaimsPrincipal> ResolveUserAsync()
        {
            // 1) Preferir HttpContext (siempre disponible en middleware/handlers)
            var httpUser = _http.HttpContext?.User;
            if (httpUser?.Identity?.IsAuthenticated == true)
                return httpUser;

            // 2) Fallback a AuthenticationStateProvider (útil en componentes Blazor)
            if (_auth is not null)
            {
                var state = await _auth.GetAuthenticationStateAsync();
                if (state.User?.Identity?.IsAuthenticated == true)
                    return state.User;
            }

            // 3) Usuario vacío si no hay autenticación
            return new ClaimsPrincipal(new ClaimsIdentity());
        }

        public async Task<ClaimsPrincipal> GetUserAsync() => await ResolveUserAsync();
        public async Task<string?> GetUserNameAsync() => (await ResolveUserAsync()).Identity?.Name;
        public async Task<string?> GetAccessTokenAsync() => (await ResolveUserAsync()).FindFirstValue("access_token");
        public async Task<string?> GetRefreshTokenAsync() => (await ResolveUserAsync()).FindFirstValue("refresh_token");
        public async Task<string?> GetRemoteUrlAsync() => (await ResolveUserAsync()).FindFirstValue("remote_url");
    }
}
