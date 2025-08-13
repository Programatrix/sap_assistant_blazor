using SAPAssistant.Exceptions;
using SAPAssistant.Models;
using SAPAssistant.Security;
using System.Net.Http.Headers;
using Microsoft.Extensions.Localization;
using SAPAssistant.Constants;
using Microsoft.JSInterop;


namespace SAPAssistant.Service
{
    public class AuthService
    {
        private readonly ApiClient _api;
        private readonly CustomAuthStateProvider _authProvider;
        private readonly IStringLocalizer<ErrorMessages> _localizer;
        private readonly IJSRuntime _js;

        public AuthService(ApiClient api, CustomAuthStateProvider authProvider, IStringLocalizer<ErrorMessages> localizer, IJSRuntime js)
        {
            _api = api;
            _authProvider = authProvider;
            _localizer = localizer;
            _js = js;
        }

        public async Task<ServiceResult<LoginResponse>> LoginAsync(LoginRequest request, bool rememberMe = false, CancellationToken ct = default)
        {
            var r = await _api.PostAsResultAsync<LoginRequest, LoginResponse>(
                "login",            // SIN /api/v1/ porque ya va en BaseAddress
                request,
                okKey: ErrorCodes.LOGIN_SUCCESS,
                ct
            );

            if (r.Success && r.Data is not null)
            {
                await _authProvider.MarkUserAsAuthenticated(r.Data.Username, r.Data.Token, rememberMe, _js);
                await _authProvider.SaveRemoteUrlAsync(r.Data.remote_url, rememberMe, _js);
            }
            return r;
        }
    }

}
