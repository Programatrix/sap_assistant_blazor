using Microsoft.Extensions.Localization;
using SAPAssistant.Exceptions;
using SAPAssistant.Service.Shared.Transport;
using SAPAssistant;

namespace Infrastructure.Mapping;

public static class EnvelopeMapper
{
    /// <summary>
    /// Adapta el envelope del backend a tu ServiceResult{T}.
    /// Reglas:
    /// - success=true → OK con mensaje localizable (okKey).
    /// - success=false → usa localizer si hay clave para errorCode; si no, usa env.Error; si tampoco hay, usa GENERIC_ERROR.
    /// </summary>
    public static ServiceResult<T> ToServiceResult<T>(
        this StdResponse<T> env,
        IStringLocalizer<ErrorMessages> localizer,
        string? correlationId = null,
        string okKey = "OK")
    {
        if (env.Success && env.Data is not null)
        {
            var ok = ServiceResult<T>.Ok(env.Data, localizer[okKey]);
            ok.ErrorCode = "OK";
            ok.CorrelationId = correlationId;
            ok.TraceId = env.TraceId;
            return ok;
        }

        var code = string.IsNullOrWhiteSpace(env.ErrorCode) ? "INTERNAL_ERROR" : env.ErrorCode!;
        var hasTranslation = localizer.GetString(code).ResourceNotFound == false;

        var message = hasTranslation
            ? localizer[code].Value
            : (!string.IsNullOrWhiteSpace(env.Error) ? env.Error! : localizer["GENERIC_ERROR"].Value);

        var fail = ServiceResult<T>.Fail(message, code);
        fail.CorrelationId = correlationId;
        fail.TraceId = env.TraceId;
        return fail;
    }
}
