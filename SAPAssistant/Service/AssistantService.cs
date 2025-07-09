using System.Net.Http.Json;
using System.Text.Json;
using SAPAssistant.Models;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace SAPAssistant.Service
{
    public class AssistantService
    {
        private readonly HttpClient _http;
        private readonly ProtectedSessionStorage _sessionStorage;

        public AssistantService(HttpClient http, ProtectedSessionStorage sessionStorage)
        {
            _http = http;
            _sessionStorage = sessionStorage;
        }

        public async Task<QueryResponse?> ConsultarAsync(string mensaje, string chatId)
        {
            var userResult = await _sessionStorage.GetAsync<string>("username");
            var connectionResult = await _sessionStorage.GetAsync<string>("active_connection_id");
            var username = userResult.Value ?? throw new Exception("Usuario no encontrado en sesión.");
            var connectionId = connectionResult.Value ?? throw new Exception("Conexión activa no encontrada.");
            var ipResult = await _sessionStorage.GetAsync<string>("remote_url");
            string remote_ip = ipResult.Value ?? throw new Exception("No se ha podido recuperar la remote_ip asociada al usuario");
            var dbtypeResult = await _sessionStorage.GetAsync<string>("active_db_type");
            string db_type = dbtypeResult.Value ?? throw new Exception("No se ha informado el tipo de base de datos activa");

            if (string.IsNullOrWhiteSpace(chatId))
                throw new ArgumentException("El chatId no puede estar vacío al enviar un mensaje.", nameof(chatId));

            var requestBody = new
            {
                mensaje,
                connection_id = connectionId,
                chat_id = chatId,
                db_type
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "/assistant/message")
            {
                Content = JsonContent.Create(requestBody)
            };

            request.Headers.Add("X-User-Id", username);
            request.Headers.Add("x-remote-ip", remote_ip);

            return await SendAndParseAsync(request);
        }

        public async Task<QueryResponse?> ConsultarDemoAsync(string mensaje)
        {
            var requestBody = new
            {
                mensaje,
                modo_demo = true,
                chat_id = "demo",
                db_type = "HANA" // o tu valor demo por defecto
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "/assistant/demo")
            {
                Content = JsonContent.Create(requestBody)
            };

            return await SendAndParseAsync(request);
        }

        private async Task<QueryResponse?> SendAndParseAsync(HttpRequestMessage request)
        {
            var response = await _http.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error en la API: {error}");
            }

            var assistantResponse = await response.Content.ReadFromJsonAsync<AssistantResponse>();

            if (assistantResponse == null)
                throw new Exception("No se pudo interpretar la respuesta del asistente.");

            return MapToQueryResponse(assistantResponse);
        }

        private static QueryResponse MapToQueryResponse(AssistantResponse assistantResponse)
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

            if (tipo == "aclaracion" || tipo == "system" || tipo == "assistant")
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
    }

}

