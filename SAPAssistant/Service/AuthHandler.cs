namespace SAPAssistant.Service
{
    // AuthHandler.cs
    using System.Net;
    using System.Net.Http.Headers;

    public class AuthHandler : DelegatingHandler
    {
        private readonly CurrentUserAccessor _current;
        private readonly AuthService _auth;

        public AuthHandler(CurrentUserAccessor current, AuthService auth)
        {
            _current = current;
            _auth = auth;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            // 1) Adjunta token actual si existe
            var token = await _current.GetAccessTokenAsync();
            if (!string.IsNullOrWhiteSpace(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await base.SendAsync(request, ct);

            // 2) Si 401, intenta refrescar y reintentar una vez
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                response.Dispose();

                var refreshed = await _auth.TryRefreshAsync(ct);
                if (!refreshed)
                    return new HttpResponseMessage(HttpStatusCode.Unauthorized);

                var newToken = await _current.GetAccessTokenAsync();
                if (!string.IsNullOrWhiteSpace(newToken))
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);

                return await base.SendAsync(CloneRequest(request), ct);
            }

            return response;
        }

        private static HttpRequestMessage CloneRequest(HttpRequestMessage req)
        {
            var clone = new HttpRequestMessage(req.Method, req.RequestUri);
            foreach (var h in req.Headers)
                clone.Headers.TryAddWithoutValidation(h.Key, h.Value);
            clone.Content = req.Content; // ok para GET/DELETE; si hay stream, recrear body
            return clone;
        }
    }

}
