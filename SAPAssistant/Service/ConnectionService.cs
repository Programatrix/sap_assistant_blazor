using SAPAssistant.Models;
using System.Net.Http.Json;
using SAPAssistant.Mapper;
using Microsoft.Extensions.Logging;
using SAPAssistant.Service.Interfaces;
using System.Net.Http.Headers;
using Microsoft.Extensions.Localization;
using SAPAssistant;
using SAPAssistant.Constants;
using SAPAssistant.Exceptions;


namespace SAPAssistant.Service
{
    public class ConnectionService : IConnectionService
    {
        private readonly HttpClient _http;
        private readonly SessionContextService _sessionContext;
        private readonly ILogger<ConnectionService> _logger;
        private readonly IStringLocalizer<ErrorMessages> _localizer;
        private readonly CurrentUserAccessor currentUserAccessor;
        public ConnectionService(HttpClient http,
                                 SessionContextService sessionContext,
                                 ILogger<ConnectionService> logger,
                                 IStringLocalizer<ErrorMessages> localizer,
                                 CurrentUserAccessor accessor)
        {
            _http = http;
            _sessionContext = sessionContext;
            _logger = logger;
            _localizer = localizer;
            currentUserAccessor = accessor;
        }

        public async Task<ServiceResult<List<ConnectionDTO>>> GetConnectionsAsync()
        {
            try
            {
                var remoteIp = await currentUserAccessor.GetRemoteUrlAsync();
                var token = await currentUserAccessor.GetAccessTokenAsync();

                if (string.IsNullOrWhiteSpace(remoteIp))
                {
                    const string code = ErrorCodes.SESSION_REMOTE_IP_NOT_FOUND;
                    return ServiceResult<List<ConnectionDTO>>.Fail(_localizer[code], code);
                }

                if (string.IsNullOrWhiteSpace(token))
                {
                    const string code = ErrorCodes.SESSION_TOKEN_NOT_FOUND;
                    return ServiceResult<List<ConnectionDTO>>.Fail(_localizer[code], code);
                }

                var request = new HttpRequestMessage(HttpMethod.Get, "/connection/user-connections");
                request.Headers.Add("x-remote-ip", remoteIp);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _http.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    const string code = ErrorCodes.CONNECTIONS_FETCH_ERROR;
                    return ServiceResult<List<ConnectionDTO>>.Fail(_localizer[code], code);
                }

                var rawList = await response.Content.ReadFromJsonAsync<List<Dictionary<string, ConnectionDTO>>>();
                var connections = ConnectionMapper.FromRawList(rawList ?? new());

                var ok = ServiceResult<List<ConnectionDTO>>.Ok(connections, _localizer[ErrorCodes.CONNECTIONS_FETCH_SUCCESS]);
                ok.ErrorCode = ErrorCodes.CONNECTIONS_FETCH_SUCCESS;
                return ok;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error de red al obtener conexiones");
                const string code = ErrorCodes.NET_ERROR;
                return ServiceResult<List<ConnectionDTO>>.Fail(_localizer[code], code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener conexiones");
                const string code = ErrorCodes.UNEXPECTED_ERROR;
                return ServiceResult<List<ConnectionDTO>>.Fail(_localizer[code], code);
            }
        }

        public async Task<ServiceResult<ConnectionDTO>> GetConnectionByIdAsync(string connectionId)
        {
            try
            {
                var remoteIp = await _sessionContext.GetRemoteIpAsync();
                var token = await _sessionContext.GetTokenAsync();

                if (string.IsNullOrWhiteSpace(remoteIp))
                {
                    const string code = ErrorCodes.SESSION_DATA_NOT_FOUND;
                    return ServiceResult<ConnectionDTO>.Fail(_localizer[code], code);
                }

                if (string.IsNullOrWhiteSpace(token))
                {
                    const string code = ErrorCodes.SESSION_TOKEN_NOT_FOUND;
                    return ServiceResult<ConnectionDTO>.Fail(_localizer[code], code);
                }

                var request = new HttpRequestMessage(HttpMethod.Get, $"/connection/connections/{connectionId}");
                request.Headers.Add("x_remote_ip", remoteIp);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _http.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    const string code = ErrorCodes.CONNECTION_FETCH_ERROR;
                    return ServiceResult<ConnectionDTO>.Fail(_localizer[code], code);
                }

                var dto = await response.Content.ReadFromJsonAsync<ConnectionDTO>();
                if (dto == null)
                {
                    const string code = ErrorCodes.EMPTY_RESPONSE;
                    return ServiceResult<ConnectionDTO>.Fail(_localizer[code], code);
                }

                var ok = ServiceResult<ConnectionDTO>.Ok(dto, _localizer[ErrorCodes.CONNECTION_FETCH_SUCCESS]);
                ok.ErrorCode = ErrorCodes.CONNECTION_FETCH_SUCCESS;
                return ok;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error de red al obtener conexión {ConnectionId}", connectionId);
                const string code = ErrorCodes.NET_ERROR;
                return ServiceResult<ConnectionDTO>.Fail(_localizer[code], code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener conexión {ConnectionId}", connectionId);
                const string code = ErrorCodes.UNEXPECTED_ERROR;
                return ServiceResult<ConnectionDTO>.Fail(_localizer[code], code);
            }
        }

        public async Task<ServiceResult> UpdateConnectionAsync(ConnectionDTO connection)
        {
            try
            {
                var remoteIp = await _sessionContext.GetRemoteIpAsync();
                var token = await _sessionContext.GetTokenAsync();
                if (string.IsNullOrWhiteSpace(remoteIp))
                {
                    const string code = ErrorCodes.SESSION_DATA_NOT_FOUND;
                    return ServiceResult.Fail(_localizer[code], code);
                }

                if (string.IsNullOrWhiteSpace(token))
                {
                    const string code = ErrorCodes.SESSION_TOKEN_NOT_FOUND;
                    return ServiceResult.Fail(_localizer[code], code);
                }

                var request = new HttpRequestMessage(HttpMethod.Put, $"/connection/connections/{connection.ConnectionId}");
                request.Headers.Add("x_remote_ip", remoteIp);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                request.Content = JsonContent.Create(connection);

                var response = await _http.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    const string code = ErrorCodes.UPDATE_CONNECTION_ERROR;
                    return ServiceResult.Fail(_localizer[code], code);
                }

                var ok = ServiceResult.Ok(_localizer[ErrorCodes.CONNECTION_UPDATED]);
                ok.ErrorCode = ErrorCodes.CONNECTION_UPDATED;
                return ok;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error de red al actualizar conexión {ConnectionId}", connection.ConnectionId);
                const string code = ErrorCodes.NET_ERROR;
                return ServiceResult.Fail(_localizer[code], code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar conexión {ConnectionId}", connection.ConnectionId);
                const string code = ErrorCodes.UNEXPECTED_ERROR;
                return ServiceResult.Fail(_localizer[code], code);
            }
        }

        public async Task<ServiceResult> CreateConnectionAsync(ConnectionDTO connection)
        {
            try
            {
                var remoteIp = await _sessionContext.GetRemoteIpAsync();
                var token = await _sessionContext.GetTokenAsync();
                if (string.IsNullOrWhiteSpace(remoteIp))
                {
                    const string code = ErrorCodes.SESSION_DATA_NOT_FOUND;
                    return ServiceResult.Fail(_localizer[code], code);
                }

                if (string.IsNullOrWhiteSpace(token))
                {
                    const string code = ErrorCodes.SESSION_TOKEN_NOT_FOUND;
                    return ServiceResult.Fail(_localizer[code], code);
                }

                var request = new HttpRequestMessage(HttpMethod.Post, "/connection/connections");
                request.Headers.Add("x_remote_ip", remoteIp);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                request.Content = JsonContent.Create(connection);

                var response = await _http.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    const string code = ErrorCodes.CREATE_CONNECTION_ERROR;
                    return ServiceResult.Fail(_localizer[code], code);
                }

                var ok = ServiceResult.Ok(_localizer[ErrorCodes.CONNECTION_CREATED]);
                ok.ErrorCode = ErrorCodes.CONNECTION_CREATED;
                return ok;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error de red al crear conexión");
                const string code = ErrorCodes.NET_ERROR;
                return ServiceResult.Fail(_localizer[code], code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear conexión");
                const string code = ErrorCodes.UNEXPECTED_ERROR;
                return ServiceResult.Fail(_localizer[code], code);
            }
        }

        public async Task<ServiceResult> ValidateConnectionAsync(string connectionId)
        {
            try
            {
                var remoteIp = await _sessionContext.GetRemoteIpAsync();
                var token = await _sessionContext.GetTokenAsync();
                if (string.IsNullOrWhiteSpace(remoteIp))
                {
                    const string code = ErrorCodes.SESSION_DATA_NOT_FOUND;
                    return ServiceResult.Fail(_localizer[code], code);
                }

                if (string.IsNullOrWhiteSpace(token))
                {
                    const string code = ErrorCodes.SESSION_TOKEN_NOT_FOUND;
                    return ServiceResult.Fail(_localizer[code], code);
                }

                var request = new HttpRequestMessage(HttpMethod.Post, $"/connection/connections/{connectionId}/validate");
                request.Headers.Add("x-remote-ip", remoteIp);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _http.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error al validar conexión {ConnectionId}: {StatusCode} - {ErrorContent}",
                                     connectionId,
                                     response.StatusCode,
                                     errorContent);
                    const string code = ErrorCodes.VALIDATION_CONNECTION_ERROR;
                    return ServiceResult.Fail(_localizer[code], code);
                }

                var ok = ServiceResult.Ok(_localizer[ErrorCodes.CONNECTION_VALID]);
                ok.ErrorCode = ErrorCodes.CONNECTION_VALID;
                return ok;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error de red al validar conexión {ConnectionId}", connectionId);
                const string code = ErrorCodes.NET_ERROR;
                return ServiceResult.Fail(_localizer[code], code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar conexión {ConnectionId}", connectionId);
                const string code = ErrorCodes.UNEXPECTED_ERROR;
                return ServiceResult.Fail(_localizer[code], code);
            }
        }

    }
}
