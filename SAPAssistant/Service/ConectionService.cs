using SAPAssistant.Models;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using SAPAssistant.Exceptions;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace SAPAssistant.Service
{
    public class ConnectionService
    {
        private readonly HttpClient _http;
        private readonly ProtectedSessionStorage _sessionStorage;

        public ConnectionService(HttpClient http, ProtectedSessionStorage sessionStorage)
        {
            _http = http;
            _sessionStorage = sessionStorage;
        }

        public async Task<ResultMessage<List<ConnectionDTO>>> GetConnectionsAsync()
        {
            try
            {
                var tokenResult = await _sessionStorage.GetAsync<string>("token");
                var userResult = await _sessionStorage.GetAsync<string>("username");

                if (!tokenResult.Success)
                    return ResultMessage<List<ConnectionDTO>>.Fail("Token no encontrado en la sesión.", "SESSION-TOKEN-NOT-FOUND");

                if (!userResult.Success)
                    return ResultMessage<List<ConnectionDTO>>.Fail("Usuario no encontrado en la sesión.", "SESSION-USER-NOT-FOUND");

                var token = tokenResult.Value;
                var userId = userResult.Value;

                // ✅ CAMBIADO el endpoint para incluir el prefijo correcto
                var request = new HttpRequestMessage(HttpMethod.Get, "/connection/user-connections");
                request.Headers.Add("X-User-Id", userId);
                // request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _http.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    return ResultMessage<List<ConnectionDTO>>.Fail(
                        "Error al obtener las conexiones desde el servidor.",
                        $"HTTP-{(int)response.StatusCode}"
                    );
                }

                var connections = await response.Content.ReadFromJsonAsync<List<ConnectionDTO>>() ?? new List<ConnectionDTO>();

                return ResultMessage<List<ConnectionDTO>>.Ok(connections, "Conexiones obtenidas exitosamente.");
            }
            catch (HttpRequestException)
            {
                return ResultMessage<List<ConnectionDTO>>.Fail("Problema de conexión. Verifique su conexión a Internet.", "NET-ERROR");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al obtener conexiones: {ex.Message}");
                return ResultMessage<List<ConnectionDTO>>.Fail("Error inesperado al obtener las conexiones.", "UNEXPECTED-ERROR");
            }
        }

        public async Task<ConnectionDTO?> GetConnectionByIdAsync(string connectionId)
        {
            var userResult = await _sessionStorage.GetAsync<string>("username");
            if (!userResult.Success)
                return null;

            var userId = userResult.Value;

            // ✅ CAMBIADO el endpoint para incluir el prefijo
            var request = new HttpRequestMessage(HttpMethod.Get, $"/connection/connections/{connectionId}");
            request.Headers.Add("X-User-Id", userId);

            var response = await _http.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<ConnectionDTO>();
        }

        public async Task<bool> UpdateConnectionAsync(ConnectionDTO connection)
        {
            var userResult = await _sessionStorage.GetAsync<string>("username");
            if (!userResult.Success) return false;

            var userId = userResult.Value;

            var request = new HttpRequestMessage(HttpMethod.Put, $"/connection/connections/{connection.ConnectionId}");
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

            var request = new HttpRequestMessage(HttpMethod.Post, "/connection/connections");
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

                var request = new HttpRequestMessage(HttpMethod.Post, $"/connection/connections/{connectionId}/validate");
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
