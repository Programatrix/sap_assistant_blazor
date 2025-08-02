using SAPAssistant.Exceptions;
using SAPAssistant.Models;
using SAPAssistant.Security;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using SAPAssistant.Resources;

namespace SAPAssistant.Service
{
    public class AuthService
    {
        private readonly HttpClient _http;
        private readonly CustomAuthStateProvider _authProvider;
        private readonly IStringLocalizer<ErrorMessages> _localizer;

        public AuthService(HttpClient http, CustomAuthStateProvider authProvider, IStringLocalizer<ErrorMessages> localizer)
        {
            _http = http;
            _authProvider = authProvider;
            _localizer = localizer;
        }

        public async Task<ResultMessage<LoginResponse>> LoginAsync(LoginRequest request)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("/login", request);
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    const string code = "AUTH-401";
                    return ResultMessage<LoginResponse>.Fail(_localizer[code], code);
                }

                if (!response.IsSuccessStatusCode)
                {
                    const string code = "SVC-UNAVAILABLE";
                    return ResultMessage<LoginResponse>.Fail(_localizer[code], code);
                }

                var user = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (user is null)
                {
                    const string code = "AUTH-INVALID-RESPONSE";
                    return ResultMessage<LoginResponse>.Fail(_localizer[code], code);
                }

                await _authProvider.MarkUserAsAuthenticated(user.Username, user.Token);
                await _authProvider.SaveRemoteUrlAsync(user.remote_url); // ✅ Guardar remote_url

                var ok = ResultMessage<LoginResponse>.Ok(user, _localizer["LOGIN-SUCCESS"]);
                ok.ErrorCode = "LOGIN-SUCCESS";
                return ok;
            }
            catch (HttpRequestException)
            {
                const string code = "NET-ERROR";
                return ResultMessage<LoginResponse>.Fail(_localizer[code], code);
            }
            catch (Exception)
            {
                const string code = "UNEXPECTED-ERROR";
                return ResultMessage<LoginResponse>.Fail(_localizer[code], code);
            }
        }

        public async Task LogoutAsync()
        {
            await _authProvider.MarkUserAsLoggedOut();
        }

        public async Task AddAuthHeaderAsync()
        {
            var token = await _authProvider.GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
    }
}
