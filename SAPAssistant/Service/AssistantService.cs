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

            var requestBody = new
            {
                mensaje = mensaje,
                connection_id = connectionId,
                chat_id = "default"
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "/assistant/message")
            {
                Content = JsonContent.Create(requestBody)
            };

            request.Headers.Add("X-User-Id", username);
            request.Headers.Add("x_remote_ip", remote_ip);

            var response = await _http.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error en la API: {error}");
            }

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            var tipo = json.GetProperty("tipo").GetString();

            if (tipo == "consulta" || tipo == "refinamiento")
            {
                return new QueryResponse
                {
                    Tipo = tipo,
                    Sql = json.GetProperty("sql").GetString(),
                    Resumen = json.GetProperty("resumen").GetString(),
                    Resultados = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(
                        json.GetProperty("resultados").GetRawText())
                };
            }
            else if (tipo == "aclaracion" || tipo == "system")
            {
                return new QueryResponse
                {
                    Tipo = tipo,
                    Mensaje = json.GetProperty("mensaje").GetString(),
                    Sql = null,
                    Resumen = null,
                    Resultados = new List<Dictionary<string, object>>()
                };
            }
            else
            {
                throw new Exception($"❌ Tipo de respuesta desconocido: {tipo}");
            }
        }
    }
}

