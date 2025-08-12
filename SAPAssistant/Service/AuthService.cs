using SAPAssistant.Exceptions;
using SAPAssistant.Models;
using SAPAssistant.Security;
using System.Net.Http.Headers;
using Microsoft.Extensions.Localization;
using SAPAssistant.Constants;


namespace SAPAssistant.Service
{
    public class AuthService
    {
        private readonly ApiClient _api;
        private readonly CustomAuthStateProvider _authProvider;
        private readonly IStringLocalizer<ErrorMessages> _localizer;

        public AuthService(ApiClient api, CustomAuthStateProvider authProvider, IStringLocalizer<ErrorMessages> localizer)
        {
            _api = api;
            _authProvider = authProvider;
            _localizer = localizer;
        }

        public async Task<ServiceResult<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default)
        {
            var r = await _api.PostAsResultAsync<LoginRequest, LoginResponse>(
                "login",            // SIN /api/v1/ porque ya va en BaseAddress
                request,
                okKey: ErrorCodes.LOGIN_SUCCESS,
                ct
            );

            if (r.Success && r.Data is not null)
            {
                await _authProvider.MarkUserAsAuthenticated(r.Data.Username, r.Data.Token);
                await _authProvider.SaveRemoteUrlAsync(r.Data.remote_url);
            }
            return r;
        }
    }

}
