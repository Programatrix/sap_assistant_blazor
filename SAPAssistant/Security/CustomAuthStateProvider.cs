using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using SAPAssistant.Service;
using System.Security.Claims;
using Microsoft.JSInterop;

namespace SAPAssistant.Security
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly SessionContextService _sessionContext;
        private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());
        private readonly ILogger<CustomAuthStateProvider> _logger;

        public CustomAuthStateProvider(SessionContextService sessionContext, ILogger<CustomAuthStateProvider> logger)
        {
            _sessionContext = sessionContext;
            _logger = logger;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var username = await _sessionContext.GetUserIdAsync();
                var token = await _sessionContext.GetTokenAsync();

                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(token))
                {
                    // ⚠️ Podés agregar más claims si querés roles, etc.
                    var identity = new ClaimsIdentity(new[]
                    {
                new Claim(ClaimTypes.Name, username),
                new Claim("access_token", token) // opcional, útil para llamadas API
            }, authenticationType: "apiauth"); // 👈 esto asegura que esté autenticado

                    var user = new ClaimsPrincipal(identity);
                    return new AuthenticationState(user);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al restaurar autenticación");
            }

            // usuario anónimo
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }


        public async Task MarkUserAsAuthenticated(string username, string token, bool persistent = false, IJSRuntime? js = null)
        {
            await _sessionContext.SetUserIdAsync(username, persistent, js);
            await _sessionContext.SetTokenAsync(token, persistent, js);

            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, username)
            }, "apiauth");

            var user = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }
        public async Task SaveRemoteUrlAsync(string remoteUrl, bool persistent = false, IJSRuntime? js = null)
        {
            await _sessionContext.SetRemoteIpAsync(remoteUrl, persistent, js);
        }
        public async Task<string?> GetRemoteUrlAsync()
        {
            return await _sessionContext.GetRemoteIpAsync();
        }

        public async Task MarkUserAsLoggedOut()
        {
            await _sessionContext.DeleteUserIdAsync();
            await _sessionContext.DeleteTokenAsync();
            await _sessionContext.DeleteRemoteIpAsync();
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
        }

        public async Task<string?> GetTokenAsync()
        {
            return await _sessionContext.GetTokenAsync();
        }
    }
}
