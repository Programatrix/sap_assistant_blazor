// Services/CurrentUserAccessor.cs
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
namespace SAPAssistant.Service
{

    public class CurrentUserAccessor
    {
        private readonly AuthenticationStateProvider _auth;

        public CurrentUserAccessor(AuthenticationStateProvider auth) => _auth = auth;

        public async Task<ClaimsPrincipal> GetUserAsync()
            => (await _auth.GetAuthenticationStateAsync()).User;

        public async Task<string?> GetUserNameAsync()
            => (await GetUserAsync()).Identity?.Name;

        public async Task<string?> GetAccessTokenAsync()
            => (await GetUserAsync()).FindFirstValue("access_token");

        public async Task<string?> GetRemoteUrlAsync()
            => (await GetUserAsync()).FindFirstValue("remote_url");
    }

}
