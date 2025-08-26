using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace SAPAssistant.Service
{
    public class CurrentUserAccessor
    {
        private readonly IHttpContextAccessor _http;

        public CurrentUserAccessor(IHttpContextAccessor http)
        {
            _http = http;
        }

        private Task<ClaimsPrincipal> ResolveUserAsync()
        {
            var user = _http.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated == true)
                return Task.FromResult(user);

            return Task.FromResult(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        public async Task<ClaimsPrincipal> GetUserAsync() => await ResolveUserAsync();
        public async Task<string?> GetUserNameAsync() => (await ResolveUserAsync()).Identity?.Name;
        public async Task<string?> GetAccessTokenAsync() => (await ResolveUserAsync()).FindFirstValue("access_token");
        public async Task<string?> GetRefreshTokenAsync() => (await ResolveUserAsync()).FindFirstValue("refresh_token");
        public async Task<string?> GetRemoteUrlAsync() => (await ResolveUserAsync()).FindFirstValue("remote_url");
    }
}
