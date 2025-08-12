using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Localization;
using Infrastructure.Mapping;
using SAPAssistant.Exceptions;
using SAPAssistant.Service.Shared.Transport;
using SAPAssistant;

public class ApiClient
{
    private readonly HttpClient _http;
    private readonly IStringLocalizer<ErrorMessages> _localizer;

    public ApiClient(HttpClient http, IStringLocalizer<ErrorMessages> localizer)
    {
        _http = http;
        _localizer = localizer;
    }

    public async Task<ResultMessage<TRes>> PostAsResultAsync<TReq, TRes>(
        string url,
        TReq body,
        string okKey = "OK",
        CancellationToken ct = default)
    {
        var corr = Guid.NewGuid().ToString();

        using var req = new HttpRequestMessage(HttpMethod.Post, url)
        { Content = JsonContent.Create(body, options: new(JsonSerializerDefaults.Web)) };
        req.Headers.TryAddWithoutValidation("x-correlation-id", corr);

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
                return env.ToResultMessage(_localizer, correlationId: corr, okKey: okKey);

            // Sin envelope → errores “físicos” del front
            if ((int)res.StatusCode is 502 or 503 or 504)
                return ResultMessage<TRes>.Fail(_localizer["SVC_UNAVAILABLE"], "SVC_UNAVAILABLE")
                                          .WithCorrelation(corr);

            return ResultMessage<TRes>.Fail(_localizer["FE_NETWORK_HTTP"], "FE_NETWORK_HTTP")
                                      .WithCorrelation(corr);
        }
        catch (TaskCanceledException)
        {
            return ResultMessage<TRes>.Fail(_localizer["FE_NETWORK_TIMEOUT"], "FE_NETWORK_TIMEOUT")
                                      .WithCorrelation(corr);
        }
        catch (HttpRequestException)
        {
            return ResultMessage<TRes>.Fail(_localizer["FE_NETWORK_ERROR"], "FE_NETWORK_ERROR")
                                      .WithCorrelation(corr);
        }
    }
}

public static class ResultMessageExt
{
    public static ResultMessage<T> WithCorrelation<T>(this ResultMessage<T> r, string corr)
    { r.CorrelationId = corr; return r; }
}
