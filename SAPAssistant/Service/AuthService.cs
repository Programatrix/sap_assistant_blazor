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

            // Si usás una API real:
            // var response = await _http.PostAsJsonAsync("https://tu-api.com/api/login", request);
            // if (response.IsSuccessStatusCode)
            // {
            //     await _authProvider.MarkUserAsAuthenticated(request.Username);
            //     return true;
            // }
            // return false;
        }
    }

}
