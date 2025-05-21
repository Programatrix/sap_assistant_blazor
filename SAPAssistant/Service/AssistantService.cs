using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using SAPAssistant.Models;

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

        public async Task<QueryResponse?> ConsultarAsync(string pregunta)
        {
            var userResult = await _sessionStorage.GetAsync<string>("username");
            var conectionresult = await _sessionStorage.GetAsync<string>("active_connection_id");
            var username = userResult.Value ?? throw new Exception("Usuario no encontrado en sesión.");

            var requestBody = new
            {
                //TODO: RECUPERAR LA CONEXIÓN ACTIVA Y EL ENGINE SE DEBERIA RECUPERAR DESDE EL MICROSERVICIO QUE EJECUTA LA QUERY
                pregunta = pregunta,
                connection_id = conectionresult.Value                  
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "/assistant/query")
            {
                Content = JsonContent.Create(requestBody)
            };

            request.Headers.Add("X-User-Id", username);

            var response = await _http.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<QueryResponse>();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error en la API: {error}");
            }
        }
    }
}
