using SAPAssistant.Exceptions;
using SAPAssistant.Models;
using SAPAssistant.Security;
using Microsoft.Extensions.Localization;
using SAPAssistant.Constants;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using System.Net.Http;


namespace SAPAssistant.Service
{
    public class AuthService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly CustomAuthStateProvider _authProvider;
        private readonly IStringLocalizer<ErrorMessages> _localizer;
        private readonly IJSRuntime _js;

        public AuthService(IHttpClientFactory httpClientFactory, IHttpContextAccessor contextAccessor,
            CustomAuthStateProvider authProvider, IStringLocalizer<ErrorMessages> localizer, IJSRuntime js)
        {
            _httpClientFactory = httpClientFactory;
            _contextAccessor = contextAccessor;
            _authProvider = authProvider;
            _localizer = localizer;
            _js = js;
        }

        public async Task<ServiceResult<LoginResponse>> LoginAsync(LoginRequest request, bool rememberMe = false, CancellationToken ct = default)
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

                if (r.Success && r.Data is not null)
                {
                    await _authProvider.MarkUserAsAuthenticated(r.Data.Username, r.Data.Token, rememberMe, _js);
                    await _authProvider.SaveRemoteUrlAsync(r.Data.remote_url, rememberMe, _js);
                }
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
    }

}
