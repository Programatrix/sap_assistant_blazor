using SAPAssistant.Models;
using System.Net.Http.Json;
using SAPAssistant.Exceptions;
using SAPAssistant.Mapper;
using Microsoft.Extensions.Logging;
using SAPAssistant.Service.Interfaces;

using Microsoft.Extensions.Localization;
using SAPAssistant;

namespace SAPAssistant.Service
{
    public class ConnectionService : IConnectionService
    {
        private readonly HttpClient _http;
        private readonly SessionContextService _sessionContext;
        private readonly ILogger<ConnectionService> _logger;
        private readonly IStringLocalizer<ErrorMessages> _localizer;

        public ConnectionService(HttpClient http,
                                 SessionContextService sessionContext,
                                 ILogger<ConnectionService> logger,
                                 IStringLocalizer<ErrorMessages> localizer)
        {
            _http = http;
            _sessionContext = sessionContext;
            _logger = logger;
            _localizer = localizer;
        }

        public async Task<ServiceResult<List<ConnectionDTO>>> GetConnectionsAsync()
        {
            try
            {
                var token = await _sessionContext.GetTokenAsync();
                var userId = await _sessionContext.GetUserIdAsync();
                var remoteIp = await _sessionContext.GetRemoteIpAsync();

                if (string.IsNullOrWhiteSpace(token))
                {
                    const string code = "SESSION-TOKEN-NOT-FOUND";
                    return ServiceResult<List<ConnectionDTO>>.Fail(_localizer[code], code);
                }

                if (string.IsNullOrWhiteSpace(userId))
                {
                    const string code = "SESSION-USER-NOT-FOUND";
                    return ServiceResult<List<ConnectionDTO>>.Fail(_localizer[code], code);
                }

                if (string.IsNullOrWhiteSpace(remoteIp))
                {
                    const string code = "SESSION-REMOTE_IP-NOT-FOUND";
                    return ServiceResult<List<ConnectionDTO>>.Fail(_localizer[code], code);
                }

                var request = new HttpRequestMessage(HttpMethod.Get, "/connection/user-connections");
                request.Headers.Add("X-User-Id", userId);
                request.Headers.Add("x-remote-ip", remoteIp);

                var response = await _http.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    const string code = "CONNECTIONS-FETCH-ERROR";
                    return ServiceResult<List<ConnectionDTO>>.Fail(_localizer[code], code);
                }

                var rawList = await response.Content.ReadFromJsonAsync<List<Dictionary<string, ConnectionDTO>>>();
                var connections = ConnectionMapper.FromRawList(rawList ?? new());

                var ok = ServiceResult<List<ConnectionDTO>>.Ok(connections, _localizer["CONNECTIONS-FETCH-SUCCESS"]);
                ok.ErrorCode = "CONNECTIONS-FETCH-SUCCESS";
                return ok;
            }
            catch (HttpRequestException)
            {
                const string code = "NET-ERROR";
                return ServiceResult<List<ConnectionDTO>>.Fail(_localizer[code], code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener conexiones");
                const string code = "UNEXPECTED-ERROR";
                return ServiceResult<List<ConnectionDTO>>.Fail(_localizer[code], code);
            }
        }

        public async Task<ServiceResult<ConnectionDTO>> GetConnectionByIdAsync(string connectionId)
        {
            try
            {
                var userId = await _sessionContext.GetUserIdAsync();
                var remoteIp = await _sessionContext.GetRemoteIpAsync();

                if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(remoteIp))
                {
                    const string code = "SESSION-DATA-NOT-FOUND";
                    return ServiceResult<ConnectionDTO>.Fail(_localizer[code], code);
                }

                var request = new HttpRequestMessage(HttpMethod.Get, $"/connection/connections/{connectionId}");
                request.Headers.Add("X-User-Id", userId);
                request.Headers.Add("x_remote_ip", remoteIp);

                var response = await _http.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    const string code = "CONNECTION-FETCH-ERROR";
                    return ServiceResult<ConnectionDTO>.Fail(_localizer[code], code);
                }

                var dto = await response.Content.ReadFromJsonAsync<ConnectionDTO>();
                if (dto == null)
                {
                    const string code = "EMPTY-RESPONSE";
                    return ServiceResult<ConnectionDTO>.Fail(_localizer[code], code);
                }

                var ok = ServiceResult<ConnectionDTO>.Ok(dto, _localizer["CONNECTION-FETCH-SUCCESS"]);
                ok.ErrorCode = "CONNECTION-FETCH-SUCCESS";
                return ok;
            }
            catch (HttpRequestException)
            {
                const string code = "NET-ERROR";
                return ServiceResult<ConnectionDTO>.Fail(_localizer[code], code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener conexión {ConnectionId}", connectionId);
                const string code = "UNEXPECTED-ERROR";
                return ServiceResult<ConnectionDTO>.Fail(_localizer[code], code);
            }
        }

        public async Task<ServiceResult> UpdateConnectionAsync(ConnectionDTO connection)
        {
            try
            {
                var userId = await _sessionContext.GetUserIdAsync();
                var remoteIp = await _sessionContext.GetRemoteIpAsync();

                if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(remoteIp))
                {
                    const string code = "SESSION-DATA-NOT-FOUND";
                    return ServiceResult.Fail(_localizer[code], code);
                }

                var request = new HttpRequestMessage(HttpMethod.Put, $"/connection/connections/{connection.ConnectionId}");
                request.Headers.Add("X-User-Id", userId);
                request.Headers.Add("x_remote_ip", remoteIp);
                request.Content = JsonContent.Create(connection);

                var response = await _http.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    const string code = "UPDATE-CONNECTION-ERROR";
                    return ServiceResult.Fail(_localizer[code], code);
                }

                var ok = ServiceResult.Ok(_localizer["CONNECTION-UPDATED"]);
                ok.ErrorCode = "CONNECTION-UPDATED";
                return ok;
            }
            catch (HttpRequestException)
            {
                const string code = "NET-ERROR";
                return ServiceResult.Fail(_localizer[code], code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar conexión {ConnectionId}", connection.ConnectionId);
                const string code = "UNEXPECTED-ERROR";
                return ServiceResult.Fail(_localizer[code], code);
            }
        }

        public async Task<ServiceResult> CreateConnectionAsync(ConnectionDTO connection)
        {
            try
            {
                var userId = await _sessionContext.GetUserIdAsync();
                var remoteIp = await _sessionContext.GetRemoteIpAsync();

                if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(remoteIp))
                {
                    const string code = "SESSION-DATA-NOT-FOUND";
                    return ServiceResult.Fail(_localizer[code], code);
                }

                var request = new HttpRequestMessage(HttpMethod.Post, "/connection/connections");
                request.Headers.Add("X-User-Id", userId);
                request.Headers.Add("x_remote_ip", remoteIp);
                request.Content = JsonContent.Create(connection);

                var response = await _http.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    const string code = "CREATE-CONNECTION-ERROR";
                    return ServiceResult.Fail(_localizer[code], code);
                }

                var ok = ServiceResult.Ok(_localizer["CONNECTION-CREATED"]);
                ok.ErrorCode = "CONNECTION-CREATED";
                return ok;
            }
            catch (HttpRequestException)
            {
                const string code = "NET-ERROR";
                return ServiceResult.Fail(_localizer[code], code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear conexión");
                const string code = "UNEXPECTED-ERROR";
                return ServiceResult.Fail(_localizer[code], code);
            }
        }

        public async Task<ServiceResult> ValidateConnectionAsync(string connectionId)
        {
            try
            {
                var userId = await _sessionContext.GetUserIdAsync();
                var remoteIp = await _sessionContext.GetRemoteIpAsync();
                if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(remoteIp))
                {
                    const string code = "SESSION-DATA-NOT-FOUND";
                    return ServiceResult.Fail(_localizer[code], code);
                }

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
                    const string code = "VALIDATION-CONNECTION-ERROR";
                    return ServiceResult.Fail(_localizer[code], code);
                }

                var ok = ServiceResult.Ok(_localizer["CONNECTION-VALID"]);
                ok.ErrorCode = "CONNECTION-VALID";
                return ok;
            }
            catch (HttpRequestException)
            {
                const string code = "NET-ERROR";
                return ServiceResult.Fail(_localizer[code], code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar conexión {ConnectionId}", connectionId);
                const string code = "UNEXPECTED-ERROR";
                return ServiceResult.Fail(_localizer[code], code);
            }
        }

    }
}
