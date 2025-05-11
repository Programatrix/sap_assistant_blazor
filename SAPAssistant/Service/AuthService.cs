using SAPAssistant.Exceptions;
using SAPAssistant.Models;
using SAPAssistant.Security;
using System.Net;
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

        public async Task<ResultMessage> LoginAsync(LoginRequest request)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("/login", request);
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return ResultMessage.Fail("Usuario o contraseña incorrectos.", "AUTH-401");
                }

                if (!response.IsSuccessStatusCode)
                {
                    return ResultMessage.Fail("El sistema no está disponible. Intenta más tarde.", $"SVC-{(int)response.StatusCode}");
                }

                var user = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (user is null)
                {
                    return ResultMessage.Fail("No se pudo procesar la respuesta del servidor.", "AUTH-INVALID-RESPONSE");
                }

                await _authProvider.MarkUserAsAuthenticated(user.Username, user.Token);
                return ResultMessage.Ok("Inicio de sesión exitoso.");
            }
            catch (HttpRequestException)
            {
                return ResultMessage.Fail("Problema de conexión. Verifique su conexión a Internet.", "NET-ERROR");
            }
            catch (Exception)
            {
                return ResultMessage.Fail("Error inesperado. Por favor, intente nuevamente.", "UNEXPECTED-ERROR");
            }

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
