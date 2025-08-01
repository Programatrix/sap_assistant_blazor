using SAPAssistant.Models;
using System.Net.Http.Json;
using SAPAssistant.Exceptions;
using Microsoft.Extensions.Logging;

namespace SAPAssistant.Service
{
    public class UserDashboardService
    {
        private readonly HttpClient _http;
        private readonly SessionContextService _sessionContext;
        private readonly ILogger<UserDashboardService> _logger;

        public UserDashboardService(HttpClient http, SessionContextService sessionContext, ILogger<UserDashboardService> logger)
        {
            _http = http;
            _sessionContext = sessionContext;
            _logger = logger;
        }

        public async Task AddKpiAsync(DashboardCardModel kpi)
        {
            var userId = await _sessionContext.GetUserIdAsync();
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogError("Usuario no encontrado en la sesión.");
                throw new DashboardServiceException("Usuario no encontrado en la sesión.");
            }

            var request = new HttpRequestMessage(HttpMethod.Post, "/user-dashboard/kpis");
            request.Headers.Add("X-User-Id", userId);
            request.Content = JsonContent.Create(kpi);

            var response = await _http.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error al agregar KPI: {StatusCode} - {Error}", response.StatusCode, error);
                throw new DashboardServiceException($"Error al agregar KPI: {response.StatusCode} - {error}");
            }
        }
    }
}
