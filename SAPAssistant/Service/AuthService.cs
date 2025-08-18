using SAPAssistant.Exceptions;
using SAPAssistant.Models;
using Microsoft.Extensions.Localization;
using SAPAssistant.Constants;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;


namespace SAPAssistant.Service
{
    public class AuthService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IStringLocalizer<ErrorMessages> _localizer;
        private readonly SessionContextService _sessionContext;
        private readonly CurrentUserAccessor currentUserAccessor;
        // === DTO para /api/v1/auth/refresh ===
        private record RefreshDto(string token_type, string access_token, string refresh_token);


        public AuthService(IHttpClientFactory httpClientFactory, IHttpContextAccessor contextAccessor,
            IStringLocalizer<ErrorMessages> localizer, SessionContextService sessionContext, CurrentUserAccessor accessor)
        {
            _httpClientFactory = httpClientFactory;
            _contextAccessor = contextAccessor;
            _localizer = localizer;
            _sessionContext = sessionContext;
            currentUserAccessor = accessor;
        }

        public async Task<ServiceResult<LoginResponse>> LoginAsync(LoginRequest request, string? csrfToken = null, CancellationToken ct = default)
        {
            var client = _httpClientFactory.CreateClient();
            var httpContext = _contextAccessor.HttpContext;
            var baseUri = $"{httpContext?.Request.Scheme}://{httpContext?.Request.Host}";
            client.BaseAddress = new Uri(baseUri);

            var token = csrfToken ?? httpContext?.Request.Cookies["XSRF-TOKEN"];
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation("X-CSRF-TOKEN", token);
                client.DefaultRequestHeaders.TryAddWithoutValidation("Cookie", $"XSRF-TOKEN={token}");
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

        private async Task UpdateCookieTokensAsync(string accessToken, string refreshToken)
        {
            var ctx = _contextAccessor.HttpContext!;
            var user = ctx.User;
            var authType = CookieAuthenticationDefaults.AuthenticationScheme;

            // 1) Obtén TODAS las identidades actuales del usuario
            var identities = user.Identities.ToList();

            // 2) Localiza la identidad del esquema de cookies (o crea una nueva si no existiera)
            var cookieId = identities.FirstOrDefault(i =>
                string.Equals(i.AuthenticationType, authType, StringComparison.Ordinal));

            if (cookieId is null)
            {
                cookieId = new ClaimsIdentity(authType);
                identities.Add(cookieId);
            }

            // 3) Upsert SOLO en la identidad de cookies
            void Upsert(ClaimsIdentity id, string type, string value)
            {
                var ex = id.FindFirst(type);
                if (ex is not null) id.RemoveClaim(ex);
                id.AddClaim(new Claim(type, value));
            }

            Upsert(cookieId, "access_token", accessToken);
            Upsert(cookieId, "refresh_token", refreshToken);

            // 4) Preserva las AuthenticationProperties originales de la cookie (persistencia, expiración, etc.)
            var authResult = await ctx.AuthenticateAsync(authType);
            var props = authResult?.Properties ?? new AuthenticationProperties();

            // 5) Reemite el principal con TODAS las identidades, solo la de cookies actualizada
            var principal = new ClaimsPrincipal(identities);
            await ctx.SignInAsync(authType, principal, props);

            // (Opcional) sincroniza tu SessionContext si lo usas
            await _sessionContext.SetTokenAsync(accessToken);
        }

        /// <summary>
        /// Intenta refrescar el access token usando el refresh token.
        /// Devuelve true si se refrescó correctamente.
        /// </summary>
        public async Task<bool> TryRefreshAsync(CancellationToken ct = default)
        {
            var refresh = await currentUserAccessor.GetRefreshTokenAsync();
            if (string.IsNullOrWhiteSpace(refresh))
                return false;

            // Usa el HttpClient "Default" que apunta a apiBaseUrl (configurado en Program.cs)
            var api = _httpClientFactory.CreateClient("Default");

            HttpResponseMessage resp;
            try
            {
                resp = await api.PostAsJsonAsync("/api/v1/auth/refresh",
                                                 new { refresh_token = refresh }, ct);
            }
            catch
            {
                return false;
            }

            if (!resp.IsSuccessStatusCode)
                return false;

            var dto = await resp.Content.ReadFromJsonAsync<RefreshDto>(cancellationToken: ct);
            if (dto is null || string.IsNullOrWhiteSpace(dto.access_token) || string.IsNullOrWhiteSpace(dto.refresh_token))
                return false;

            await UpdateCookieTokensAsync(dto.access_token, dto.refresh_token);
            return true;
        }
    }
}

