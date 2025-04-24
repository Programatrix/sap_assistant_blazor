using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Security.Claims;

namespace SAPAssistant.Security
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ProtectedSessionStorage _sessionStorage;
        private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

        public CustomAuthStateProvider(ProtectedSessionStorage sessionStorage)
        {
            _sessionStorage = sessionStorage;
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
                Console.WriteLine("❌ Error al restaurar autenticación: " + ex.Message);
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

        public async Task MarkUserAsLoggedOut()
        {
            await _sessionStorage.DeleteAsync("username");
            await _sessionStorage.DeleteAsync("token");
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
        }

        public async Task<string?> GetTokenAsync()
        {
            var result = await _sessionStorage.GetAsync<string>("token");
            return result.Success ? result.Value : null;
        }
    }
}
