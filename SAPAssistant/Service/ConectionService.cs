    using System.Net.Http.Headers;
    using System.Net.Http.Json;
    using SAPAssistant.Models;
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

        public async Task<List<string>> GetConnectionsAsync()
        {
            var tokenResult = await _sessionStorage.GetAsync<string>("token");
            if (!tokenResult.Success || string.IsNullOrEmpty(tokenResult.Value))
                return new List<string>();

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenResult.Value);

            var response = await _http.GetAsync("/connections"); // Ruta expuesta por el microservicio de conexiones
            if (!response.IsSuccessStatusCode)
                return new List<string>();

            return await response.Content.ReadFromJsonAsync<List<string>>() ?? new List<string>(); ;
        }
    }

}
