    using System.Net.Http.Headers;
    using System.Net.Http.Json;
    using SAPAssistant.Models;
    using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
namespace SAPAssistant.Service
{

    using System.Net.Http.Headers;
    using System.Net.Http.Json;
    using SAPAssistant.Models;
    using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

    public class ConnectionService
    {
        private readonly HttpClient _http;
        private readonly ProtectedSessionStorage _sessionStorage;

        public ConnectionService(HttpClient http, ProtectedSessionStorage sessionStorage)
        {
            _http = http;
            _sessionStorage = sessionStorage;
        }

        public async Task<List<ConnectionDTO>> GetConnectionsAsync()
        {
            try
            {
                var tokenResult = await _sessionStorage.GetAsync<string>("token");
                var userResult = await _sessionStorage.GetAsync<string>("username");

                if (!tokenResult.Success || !userResult.Success)
                    return new List<ConnectionDTO>();

                var token = tokenResult.Value;
                var userId = userResult.Value;

                var request = new HttpRequestMessage(HttpMethod.Get, "/user-connections");
                request.Headers.Add("X-User-Id", userId);
                // Si tu backend empieza a requerir token en el futuro:
                // request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _http.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                    return new List<ConnectionDTO>();

                return await response.Content.ReadFromJsonAsync<List<ConnectionDTO>>() ?? new List<ConnectionDTO>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al obtener conexiones: {ex.Message}");
                return new List<ConnectionDTO>();
            }
        }

        public async Task<bool> UpdateConnectionAsync(ConnectionDTO connection)
        {
            var userResult = await _sessionStorage.GetAsync<string>("username");
            if (!userResult.Success) return false;

            var userId = userResult.Value;

            var request = new HttpRequestMessage(HttpMethod.Put, $"/connections/{connection.ConnectionId}");
            request.Headers.Add("X-User-Id", userId);
            request.Content = JsonContent.Create(connection);

            var response = await _http.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> CreateConnectionAsync(ConnectionDTO connection)
        {
            var userResult = await _sessionStorage.GetAsync<string>("username");
            if (!userResult.Success) return false;

            var userId = userResult.Value;

            var request = new HttpRequestMessage(HttpMethod.Post, "/connections");
            request.Headers.Add("X-User-Id", userId);
            request.Content = JsonContent.Create(connection);

            var response = await _http.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ValidateConnectionAsync(string connectionId)
        {
            try
            {
                var userResult = await _sessionStorage.GetAsync<string>("username");
                if (!userResult.Success)
                    return false;

                var userId = userResult.Value;

                var request = new HttpRequestMessage(HttpMethod.Post, $"/connections/{connectionId}/validate");
                request.Headers.Add("X-User-Id", userId);

                var response = await _http.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al validar conexión {connectionId}: {ex.Message}");
                return false;
            }
        }



    }

}
