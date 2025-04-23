using SAPAssistant.Models;
using SAPAssistant.Security;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

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
            var response = await _http.PostAsJsonAsync("/login", request);
            if (!response.IsSuccessStatusCode) return false;

            var user = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if (user is null) return false;

            await _authProvider.MarkUserAsAuthenticated(user.Username, user.Token);
            return true;
        }

        public async Task LogoutAsync()
        {
            await _authProvider.MarkUserAsLoggedOut();
        }

        public async Task AddAuthHeaderAsync()
        {
            var token = await _authProvider.GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
    }
}
