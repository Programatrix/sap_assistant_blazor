using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace SAPAssistant.Security
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ProtectedSessionStorage _sessionStorage;
        private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());
        private readonly ILogger<CustomAuthStateProvider> _logger;

        public CustomAuthStateProvider(ProtectedSessionStorage sessionStorage, ILogger<CustomAuthStateProvider> logger)
        {
            _sessionStorage = sessionStorage;
            _logger = logger;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var usernameResult = await _sessionStorage.GetAsync<string>("username");
                var tokenResult = await _sessionStorage.GetAsync<string>("token");

                var username = usernameResult.Success ? usernameResult.Value : null;
                var token = tokenResult.Success ? tokenResult.Value : null;

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


        public async Task MarkUserAsAuthenticated(string username, string token)
        {
            await _sessionStorage.SetAsync("username", username);
            await _sessionStorage.SetAsync("token", token);

            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, username)
            }, "apiauth");

            var user = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }
        public async Task SaveRemoteUrlAsync(string remoteUrl)
        {
            await _sessionStorage.SetAsync("remote_url", remoteUrl);
        }
        public async Task<string?> GetRemoteUrlAsync()
        {
            var result = await _sessionStorage.GetAsync<string>("remote_url");
            return result.Success ? result.Value : null;
        }

        public async Task MarkUserAsLoggedOut()
        {
            await _sessionStorage.DeleteAsync("username");
            await _sessionStorage.DeleteAsync("token");
            await _sessionStorage.DeleteAsync("remote_url");
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
        }

        public async Task<string?> GetTokenAsync()
        {
            var result = await _sessionStorage.GetAsync<string>("token");
            return result.Success ? result.Value : null;
        }
    }
}
