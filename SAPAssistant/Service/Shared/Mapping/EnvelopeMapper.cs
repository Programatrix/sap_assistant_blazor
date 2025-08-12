using Microsoft.Extensions.Localization;
using SAPAssistant.Exceptions;
using SAPAssistant.Service.Shared.Transport;
using SAPAssistant;
using SAPAssistant.Constants;

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
        string okKey = ErrorCodes.OK)
    {
        if (env.Success && env.Data is not null)
        {
            var type = env.NotificationType ?? NotificationType.Success;
            var ok = ServiceResult<T>.Ok(env.Data, localizer[okKey], type);
            ok.ErrorCode = ErrorCodes.OK;
            ok.CorrelationId = correlationId;
            ok.TraceId = env.TraceId;
            return ok;
        }

        var code = string.IsNullOrWhiteSpace(env.ErrorCode) ? ErrorCodes.INTERNAL_ERROR : env.ErrorCode!;
        var hasTranslation = localizer.GetString(code).ResourceNotFound == false;

        var message = hasTranslation
            ? localizer[code].Value
            : (!string.IsNullOrWhiteSpace(env.Error) ? env.Error! : localizer[ErrorCodes.GENERIC_ERROR].Value);

        var failType = env.NotificationType ?? NotificationType.Error;
        var fail = ServiceResult<T>.Fail(message, code, failType);
        fail.CorrelationId = correlationId;
        fail.TraceId = env.TraceId;
        return fail;
    }
}
