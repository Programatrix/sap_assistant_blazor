using SAPAssistant.Exceptions;
using SAPAssistant.Models;
using Microsoft.Extensions.Localization;
using SAPAssistant.Constants;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using System.Net.Http;


namespace SAPAssistant.Service
{
    public class AuthService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IStringLocalizer<ErrorMessages> _localizer;
        private readonly SessionContextService _sessionContext;

        public AuthService(IHttpClientFactory httpClientFactory, IHttpContextAccessor contextAccessor,
            IStringLocalizer<ErrorMessages> localizer, SessionContextService sessionContext)
        {
            _httpClientFactory = httpClientFactory;
            _contextAccessor = contextAccessor;
            _localizer = localizer;
            _sessionContext = sessionContext;
        }

        public async Task<ServiceResult<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default)
        {
            var client = _httpClientFactory.CreateClient();
            var httpContext = _contextAccessor.HttpContext;
            var baseUri = $"{httpContext?.Request.Scheme}://{httpContext?.Request.Host}";
            client.BaseAddress = new Uri(baseUri);

            var token = httpContext?.Request.Cookies["XSRF-TOKEN"];
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation("X-CSRF-TOKEN", token);
            }

            try
            {
                var response = await client.PostAsJsonAsync("/auth/login", request, ct);
                var r = await response.Content.ReadFromJsonAsync<ServiceResult<LoginResponse>>(cancellationToken: ct)
                    ?? ServiceResult<LoginResponse>.Fail(_localizer[ErrorCodes.FE_NETWORK_HTTP], ErrorCodes.FE_NETWORK_HTTP);
                return r;
            }
            catch (TaskCanceledException)
            {
                return ServiceResult<LoginResponse>.Fail(_localizer[ErrorCodes.FE_NETWORK_TIMEOUT], ErrorCodes.FE_NETWORK_TIMEOUT);
            }
            catch
            {
                return ServiceResult<LoginResponse>.Fail(_localizer[ErrorCodes.FE_NETWORK_ERROR], ErrorCodes.FE_NETWORK_ERROR);
            }
        }

        public async Task LogoutAsync(CancellationToken ct = default)
        {
            var client = _httpClientFactory.CreateClient();
            var httpContext = _contextAccessor.HttpContext;
            var baseUri = $"{httpContext?.Request.Scheme}://{httpContext?.Request.Host}";
            client.BaseAddress = new Uri(baseUri);

            var token = httpContext?.Request.Cookies["XSRF-TOKEN"];
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation("X-CSRF-TOKEN", token);
            }

            try
            {
                await client.PostAsync("/auth/logout", null, ct);
            }
            catch
            {
                // ignore errors on logout
            }
            finally
            {
                await _sessionContext.DeleteTokenAsync();
                await _sessionContext.DeleteUserIdAsync();
                await _sessionContext.DeleteRemoteIpAsync();
                await _sessionContext.DeleteActiveConnectionIdAsync();
                await _sessionContext.DeleteDatabaseTypeAsync();
            }
        }
    }

}
