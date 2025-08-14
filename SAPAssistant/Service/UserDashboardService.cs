using SAPAssistant.Models;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Localization;
using SAPAssistant;
using SAPAssistant.Constants;
using SAPAssistant.Service.Interfaces;
using System.Net.Http.Headers;
using SAPAssistant.Exceptions;

namespace SAPAssistant.Service
{
    public class UserDashboardService : IUserDashboardService
    {
        private readonly HttpClient _http;
        private readonly SessionContextService _sessionContext;
        private readonly ILogger<UserDashboardService> _logger;
        private readonly IStringLocalizer<ErrorMessages> _localizer;

        public UserDashboardService(HttpClient http,
                                    SessionContextService sessionContext,
                                    ILogger<UserDashboardService> logger,
                                    IStringLocalizer<ErrorMessages> localizer)
        {
            _http = http;
            _sessionContext = sessionContext;
            _logger = logger;
            _localizer = localizer;
        }

        public async Task<ServiceResult> AddKpiAsync(DashboardCardModel kpi)
        {
            try
            {
                var token = await _sessionContext.GetTokenAsync();
                if (string.IsNullOrWhiteSpace(token))
                {
                    const string code = ErrorCodes.SESSION_TOKEN_NOT_FOUND;
                    return ServiceResult.Fail(_localizer[code], code);
                }

                var request = new HttpRequestMessage(HttpMethod.Post, "/user-dashboard/kpis");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
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
                const string code = ErrorCodes.UNEXPECTED_ERROR;
                return ServiceResult.Fail(_localizer[code], code);
            }
        }
    }
}
