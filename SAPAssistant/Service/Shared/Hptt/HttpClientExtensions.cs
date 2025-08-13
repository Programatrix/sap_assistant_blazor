using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Localization;
using Infrastructure.Mapping;
using SAPAssistant.Exceptions;
using SAPAssistant.Service.Shared.Transport;
using SAPAssistant;
using SAPAssistant.Constants;
using Microsoft.AspNetCore.Http;


public class ApiClient
{
    private readonly HttpClient _http;
    private readonly IStringLocalizer<ErrorMessages> _localizer;
    private readonly IHttpContextAccessor _contextAccessor;

    public ApiClient(HttpClient http, IStringLocalizer<ErrorMessages> localizer, IHttpContextAccessor contextAccessor)
    {
        _http = http;
        _localizer = localizer;
        _contextAccessor = contextAccessor;
    }

    private void AttachCsrfToken(HttpRequestMessage req)
    {
        var token = _contextAccessor.HttpContext?.Request.Cookies["XSRF-TOKEN"];
        if (!string.IsNullOrEmpty(token))
        {
            req.Headers.TryAddWithoutValidation("X-CSRF-TOKEN", token);
        }
    }

    public async Task<ServiceResult<TRes>> PostAsResultAsync<TReq, TRes>(
        string url,
        TReq body,
        string okKey = ErrorCodes.OK,
        CancellationToken ct = default)
    {
        var corr = Guid.NewGuid().ToString();

        using var req = new HttpRequestMessage(HttpMethod.Post, url)
        { Content = JsonContent.Create(body, options: new(JsonSerializerDefaults.Web)) };
        req.Headers.TryAddWithoutValidation("x-correlation-id", corr);
        AttachCsrfToken(req);

        try
        {
            using var res = await _http.SendAsync(req, ct);

            // Intenta LEER ENVELOPE SIEMPRE (aunque HTTP sea 4xx/5xx)
            StdResponse<TRes>? env = null;
            try
            {
                env = await res.Content.ReadFromJsonAsync<StdResponse<TRes>>(cancellationToken: ct);
            }
            catch { /* respuesta no parseable → trataremos abajo */ }

            if (env is not null)
                return env.ToServiceResult(_localizer, correlationId: corr, okKey: okKey);

            // Sin envelope → errores “físicos” del front
            if ((int)res.StatusCode is 502 or 503 or 504)
                return ServiceResult<TRes>.Fail(_localizer[ErrorCodes.SVC_UNAVAILABLE], ErrorCodes.SVC_UNAVAILABLE)
                                          .WithCorrelation(corr)
                                          .WithTrace(corr);

            return ServiceResult<TRes>.Fail(_localizer[ErrorCodes.FE_NETWORK_HTTP], ErrorCodes.FE_NETWORK_HTTP)
                                      .WithCorrelation(corr)
                                      .WithTrace(corr);
        }
        catch (TaskCanceledException)
        {
            return ServiceResult<TRes>.Fail(_localizer[ErrorCodes.FE_NETWORK_TIMEOUT], ErrorCodes.FE_NETWORK_TIMEOUT)
                                      .WithCorrelation(corr)
                                      .WithTrace(corr);
        }
        catch (HttpRequestException)
        {
            return ServiceResult<TRes>.Fail(_localizer[ErrorCodes.FE_NETWORK_ERROR], ErrorCodes.FE_NETWORK_ERROR)
                                      .WithCorrelation(corr)
                                      .WithTrace(corr);
        }
    }
}

