using System.Net.Http.Json;
using System.Text.Json;
using SAPAssistant.Models;

namespace SAPAssistant.Service
{
    public class AssistantService
    {
        private readonly HttpClient _http;
        private readonly SessionContextService _sessionContext;
        private readonly ILogger<AssistantService> _logger;

        public AssistantService(HttpClient http,
                              SessionContextService sessionContext,
                              ILogger<AssistantService> logger)
        {
            _http = http;
            _sessionContext = sessionContext;
            _logger = logger;
        }

        public async Task<QueryResponse?> ConsultarAsync(string mensaje, string chatId)
        {
            try
            {
                var username = await _sessionContext.GetUserIdAsync() ??
                    throw new AssistantException("Usuario no autenticado", "UNAUTHENTICATED");
                var connectionId = await _sessionContext.GetActiveConnectionIdAsync() ??
                    throw new AssistantException("No hay conexión activa", "NO_ACTIVE_CONNECTION");
                var remoteIp = await _sessionContext.GetRemoteIpAsync() ??
                    throw new AssistantException("Configuración remota faltante", "MISSING_REMOTE_IP");
                var dbType = await _sessionContext.GetDatabaseTypeAsync() ??
                    throw new AssistantException("Tipo de base de datos no especificado", "MISSING_DB_TYPE");

                if (string.IsNullOrWhiteSpace(chatId))
                    throw new AssistantException("ID de chat inválido", "INVALID_CHAT_ID");

                var request = new HttpRequestMessage(HttpMethod.Post, "/assistant/message")
                {
                    Content = JsonContent.Create(new
                    {
                        mensaje,
                        connection_id = connectionId,
                        chat_id = chatId,
                        db_type = dbType
                    })
                };

                request.Headers.Add("X-User-Id", username);
                request.Headers.Add("x-remote-ip", remoteIp);

                return await SendAndParseAsync(request);
            }
            catch (AssistantException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado en ConsultarAsync");
                throw new AssistantException("Error interno del cliente", "CLIENT_ERROR");
            }
        }

        public async Task<QueryResponse?> ConsultarDemoAsync(string mensaje)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, "/assistant/demo")
                {
                    Content = JsonContent.Create(new
                    {
                        mensaje,
                        modo_demo = true,
                        chat_id = "demo",
                        db_type = "HANA"
                    })
                };

                return await SendAndParseAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ConsultarDemoAsync");
                throw new AssistantException("Error en modo demo", "DEMO_ERROR");
            }
        }

        private async Task<QueryResponse?> SendAndParseAsync(HttpRequestMessage request)
        {
            try
            {
                var response = await _http.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    await HandleErrorResponse(response);
                }

                var assistantResponse = await response.Content.ReadFromJsonAsync<AssistantResponse>();

                if (assistantResponse == null)
                {
                    throw new AssistantException("Respuesta inválida del servidor", "INVALID_RESPONSE");
                }

                return MapToQueryResponse(assistantResponse);
            }
            catch (AssistantException)
            {
                throw;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error al parsear JSON");
                throw new AssistantException("Error en formato de respuesta", "RESPONSE_FORMAT_ERROR");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error de conexión");
                throw new AssistantException("Error de conexión con el servidor", "CONNECTION_ERROR");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado");
                throw new AssistantException("Error interno", "INTERNAL_ERROR");
            }
        }

        private async Task HandleErrorResponse(HttpResponseMessage response)
        {
            var errorText = await response.Content.ReadAsStringAsync();
            var requestId = GetRequestId(response);

            try
            {
                using var doc = JsonDocument.Parse(errorText);
                var root = doc.RootElement;

                if (root.TryGetProperty("error", out var errorObj))
                {
                    var message = errorObj.GetProperty("message").GetString() ?? "Error del servidor";
                    var code = errorObj.GetProperty("code").GetString() ?? "SERVER_ERROR";
                    throw new AssistantException(message, code, requestId);
                }

                throw new AssistantException(errorText, "UNKNOWN_ERROR", requestId);
            }
            catch (JsonException)
            {
                throw new AssistantException($"Error del servidor: {errorText}", "SERVER_ERROR", requestId);
            }
        }

        private string? GetRequestId(HttpResponseMessage response)
        {
            return response.Headers.TryGetValues("X-Request-ID", out var values)
                   ? values.FirstOrDefault()
                   : null;
        }

        private static QueryResponse MapToQueryResponse(AssistantResponse assistantResponse)
        {
            try
            {
                var tipo = assistantResponse.Tipo ?? "system";
                string? sql = assistantResponse.Sql;
                string? resumen = null;
                string? mensaje = null;
                List<Dictionary<string, object>>? resultados = null;
                string viewType = "grid";

                if (assistantResponse.Meta != null &&
                    assistantResponse.Meta.TryGetValue("view_type", out var vtObj) &&
                    vtObj != null)
                {
                    viewType = vtObj.ToString() ?? "grid";
                }

                if (assistantResponse.Data.HasValue)
                {
                    var data = assistantResponse.Data.Value;

                    switch (data.ValueKind)
                    {
                        case JsonValueKind.Object:
                            if (data.TryGetProperty("sql", out var sqlElement))
                                sql = sqlElement.GetString();

                            if (data.TryGetProperty("resultado", out var resultadoElement) &&
                                resultadoElement.ValueKind == JsonValueKind.Array)
                            {
                                resultados = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(resultadoElement.GetRawText());
                            }
                            break;

                        case JsonValueKind.Array:
                            resultados = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(data.GetRawText());
                            break;
                    }
                }

                if (tipo == "resumen" && assistantResponse.Tool == "GenerarResumenDesdeDatos")
                    resumen = assistantResponse.Mensaje;

                if (tipo == "aclaracion" || tipo == "system" || tipo == "assistant" || tipo == "consulta")
                    mensaje = assistantResponse.Mensaje;

                return new QueryResponse
                {
                    Tipo = tipo,
                    Sql = sql,
                    Resumen = resumen,
                    Mensaje = mensaje,
                    Resultados = resultados,
                    ViewType = viewType
                };
            }
            catch (Exception ex)
            {
                throw new AssistantException($"Error al mapear respuesta: {ex.Message}", "RESPONSE_MAPPING_ERROR");
            }
        }
    }

    public class AssistantException : Exception
    {
        public string ErrorCode { get; }
        public string? RequestId { get; }
        public string? DocumentationUrl { get; }

        public AssistantException(string message, string errorCode,
                                string? requestId = null,
                                string? documentationUrl = null)
            : base(message)
        {
            ErrorCode = errorCode;
            RequestId = requestId;
            DocumentationUrl = documentationUrl;
        }

        public override string ToString()
        {
            return $"[{ErrorCode}] {Message} (RequestId: {RequestId})";
        }
    }
}