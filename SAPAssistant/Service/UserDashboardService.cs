using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using SAPAssistant.Models;
using System.Net.Http.Json;

namespace SAPAssistant.Service
{
    public class UserDashboardService
    {
        private readonly HttpClient _http;
        private readonly ProtectedSessionStorage _sessionStorage;

        public UserDashboardService(HttpClient http, ProtectedSessionStorage sessionStorage)
        {
            _http = http;
            _sessionStorage = sessionStorage;
        }

        private async Task<string> GetUserIdAsync()
        {
            var userResult = await _sessionStorage.GetAsync<string>("username");
            if (!userResult.Success)
                throw new Exception("Usuario no encontrado en la sesión.");
            return userResult.Value!;
        }

        public async Task AddKpiAsync(DashboardCardModel kpi)
        {
            var userId = await GetUserIdAsync();
            var request = new HttpRequestMessage(HttpMethod.Post, "/user-dashboard/kpis");
            request.Headers.Add("X-User-Id", userId);
            request.Content = JsonContent.Create(kpi);

            var response = await _http.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error al agregar KPI: {response.StatusCode} - {error}");
            }
        }
    }
}
