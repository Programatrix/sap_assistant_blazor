using SAPAssistant.Models;
using SAPAssistant.Security;

namespace SAPAssistant.Service
{
    public class AuthService
    {
        private readonly HttpClient _http;
        private readonly CustomAuthStateProvider _authProvider;

        public AuthService(HttpClient http, CustomAuthStateProvider authProvider)
        {
            _http = http;
            _authProvider = authProvider;
        }

        public async Task<bool> LoginAsync(LoginRequest request)
        {
            // Simulación (reemplazalo por tu llamada real)
            if (request.Username == "admin" && request.Password == "1234")
            {
                await _authProvider.MarkUserAsAuthenticated(request.Username); // ✅ MARCA AUTENTICADO
                return true;
            }

            return false;
        }

        public async Task LogoutAsync()
        {
            await _authProvider.MarkUserAsLoggedOut(); // 👈 Implementaremos este método en el provider
        }
    }

}
