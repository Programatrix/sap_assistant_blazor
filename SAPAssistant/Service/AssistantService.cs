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

        public async Task<QueryResponse?> ConsultarAsync(string mensaje)
        {
            var userResult = await _sessionStorage.GetAsync<string>("username");
            var connectionResult = await _sessionStorage.GetAsync<string>("active_connection_id");
            var username = userResult.Value ?? throw new Exception("Usuario no encontrado en sesión.");
            var connectionId = connectionResult.Value ?? throw new Exception("Conexión activa no encontrada.");
            var ipResult = await _sessionStorage.GetAsync<string>("remote_url");
            string remote_ip = ipResult.Value ?? throw new Exception("No se ha podido recuperar la remote_ip asociada al usuario");
            var dbtypeResult = await _sessionStorage.GetAsync<string>("active_db_type");
            string db_type = dbtypeResult.Value ?? throw new Exception("No se ha informado el tipo de base de datos activa");

            var requestBody = new
            {
                mensaje = mensaje,
                connection_id = connectionId,
                chat_id = "default",
                db_type
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "/assistant/message")
            {
                Content = JsonContent.Create(requestBody)
            };

            request.Headers.Add("X-User-Id", username);
            request.Headers.Add("x-remote-ip", remote_ip);

            var response = await _http.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error en la API: {error}");
            }

            var assistantResponse = await response.Content.ReadFromJsonAsync<AssistantResponse>();

            if (assistantResponse == null)
                throw new Exception("No se pudo interpretar la respuesta del asistente.");

            var tipo = assistantResponse.Tipo ?? "system"; // Default si viene null

            // 🔁 Mapeamos a tu modelo QueryResponse
            return new QueryResponse
            {
                Tipo = assistantResponse.Tipo,
                Sql = assistantResponse.Data != null && assistantResponse.Data.ContainsKey("sql")
                    ? assistantResponse.Data["sql"]?.ToString()
                    : null,

                            Resumen = assistantResponse.Tipo == "respuesta" && assistantResponse.Tool == "GenerarResumenDesdeDatos"
                    ? assistantResponse.Mensaje
                    : null,

                            Mensaje = assistantResponse.Tipo == "aclaracion"
                       || assistantResponse.Tipo == "system"
                       || assistantResponse.Tipo == "asistente"
                    ? assistantResponse.Mensaje
                    : null,

                            Resultados = assistantResponse.Data != null && assistantResponse.Data.ContainsKey("resultado")
                    ? JsonSerializer.Deserialize<List<Dictionary<string, object>>>(
                          assistantResponse.Data["resultado"].ToString()
                      )
                    : null
                        };

        }

    }
}

