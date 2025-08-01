using SAPAssistant.Models;
using System.Net.Http.Json;
using SAPAssistant.Exceptions;
using SAPAssistant.Mapper;
using Microsoft.Extensions.Logging;
using SAPAssistant.Service.Interfaces;

namespace SAPAssistant.Service
{
    public class ConnectionService : IConnectionService
    {
        private readonly HttpClient _http;
        private readonly SessionContextService _sessionContext;
        private readonly ILogger<ConnectionService> _logger;

        public ConnectionService(HttpClient http,
                                 SessionContextService sessionContext,
                                 ILogger<ConnectionService> logger)
        {
            _http = http;
            _sessionContext = sessionContext;
            _logger = logger;
        }

        public async Task<ResultMessage<List<ConnectionDTO>>> GetConnectionsAsync()
        {
            try
            {
                var token = await _sessionContext.GetTokenAsync();
                var userId = await _sessionContext.GetUserIdAsync();
                var remoteIp = await _sessionContext.GetRemoteIpAsync();

                if (string.IsNullOrWhiteSpace(token))
                    return ResultMessage<List<ConnectionDTO>>.Fail("Token no encontrado en la sesión.", "SESSION-TOKEN-NOT-FOUND");

                if (string.IsNullOrWhiteSpace(userId))
                    return ResultMessage<List<ConnectionDTO>>.Fail("Usuario no encontrado en la sesión.", "SESSION-USER-NOT-FOUND");

                if (string.IsNullOrWhiteSpace(remoteIp))
                    return ResultMessage<List<ConnectionDTO>>.Fail("Ip remota del usuario no encontrada en la sesión.", "SESSION-REMOTE_IP-NOT-FOUND");

                var request = new HttpRequestMessage(HttpMethod.Get, "/connection/user-connections");
                request.Headers.Add("X-User-Id", userId);
                request.Headers.Add("x-remote-ip", remoteIp);

                var response = await _http.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    return ResultMessage<List<ConnectionDTO>>.Fail(
                        "Error al obtener las conexiones desde el servidor.",
                        $"HTTP-{(int)response.StatusCode}"
                    );
                }

                var rawList = await response.Content.ReadFromJsonAsync<List<Dictionary<string, ConnectionDTO>>>();
                var connections = ConnectionMapper.FromRawList(rawList ?? new());

                return ResultMessage<List<ConnectionDTO>>.Ok(connections, "Conexiones obtenidas exitosamente.");
            }
            catch (HttpRequestException)
            {
                return ResultMessage<List<ConnectionDTO>>.Fail("Problema de conexión. Verifique su conexión a Internet.", "NET-ERROR");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener conexiones");
                return ResultMessage<List<ConnectionDTO>>.Fail("Error inesperado al obtener las conexiones.", "UNEXPECTED-ERROR");
            }
        }

        public async Task<ConnectionDTO?> GetConnectionByIdAsync(string connectionId)
        {
            var userId = await _sessionContext.GetUserIdAsync();
            var remoteIp = await _sessionContext.GetRemoteIpAsync();

            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(remoteIp))
                return null;

            var request = new HttpRequestMessage(HttpMethod.Get, $"/connection/connections/{connectionId}");
            request.Headers.Add("X-User-Id", userId);
            request.Headers.Add("x_remote_ip", remoteIp);

            var response = await _http.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<ConnectionDTO>();
        }

        public async Task<bool> UpdateConnectionAsync(ConnectionDTO connection)
        {
            var userId = await _sessionContext.GetUserIdAsync();
            var remoteIp = await _sessionContext.GetRemoteIpAsync();

            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(remoteIp)) return false;

            var request = new HttpRequestMessage(HttpMethod.Put, $"/connection/connections/{connection.ConnectionId}");
            request.Headers.Add("X-User-Id", userId);
            request.Headers.Add("x_remote_ip", remoteIp);
            request.Content = JsonContent.Create(connection);

            var response = await _http.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> CreateConnectionAsync(ConnectionDTO connection)
        {
            var userId = await _sessionContext.GetUserIdAsync();
            var remoteIp = await _sessionContext.GetRemoteIpAsync();

            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(remoteIp)) return false;

            var request = new HttpRequestMessage(HttpMethod.Post, "/connection/connections");
            request.Headers.Add("X-User-Id", userId);
            request.Headers.Add("x_remote_ip", remoteIp);
            request.Content = JsonContent.Create(connection);

            var response = await _http.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ValidateConnectionAsync(string connectionId)
        {
            try
            {
                var userId = await _sessionContext.GetUserIdAsync();
                var remoteIp = await _sessionContext.GetRemoteIpAsync();
                if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(remoteIp))
                    return false;

                var request = new HttpRequestMessage(HttpMethod.Post, $"/connection/connections/{connectionId}/validate");
                request.Headers.Add("X-User-Id", userId);
                request.Headers.Add("x-remote-ip", remoteIp);

                var response = await _http.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error al validar conexión {ConnectionId}: {StatusCode} - {ErrorContent}",
                                     connectionId,
                                     response.StatusCode,
                                     errorContent);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar conexión {ConnectionId}", connectionId);
                return false;
            }
        }

    }
}
