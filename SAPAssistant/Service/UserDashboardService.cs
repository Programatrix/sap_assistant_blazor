using SAPAssistant.Models;
using System.Net.Http.Json;
using SAPAssistant.Exceptions;
using Microsoft.Extensions.Logging;
using SAPAssistant.Service.Interfaces;

namespace SAPAssistant.Service
{
    public class UserDashboardService : IUserDashboardService
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

        public async Task<ServiceResult> AddKpiAsync(DashboardCardModel kpi)
        {
            try
            {
                var userId = await _sessionContext.GetUserIdAsync();
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogError("Usuario no encontrado en la sesión.");
                    return ServiceResult.Fail("Usuario no encontrado en la sesión.", "SESSION-USER-NOT-FOUND");
                }

                var request = new HttpRequestMessage(HttpMethod.Post, "/user-dashboard/kpis");
                request.Headers.Add("X-User-Id", userId);
                request.Content = JsonContent.Create(kpi);

                var response = await _http.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error al agregar KPI: {StatusCode} - {Error}", response.StatusCode, error);
                    return ServiceResult.Fail($"Error al agregar KPI: {response.StatusCode} - {error}");
                }

                return ServiceResult.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar KPI");
                return ServiceResult.Fail("Error al agregar KPI");
            }
        }
    }
}
