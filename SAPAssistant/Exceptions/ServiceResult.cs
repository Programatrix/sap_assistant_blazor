using System;

namespace SAPAssistant.Exceptions;

public class ServiceResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
    public NotificationType NotificationType { get; set; } = NotificationType.Success;
    public string? CorrelationId { get; set; }
    public string? TraceId { get; set; }

    public static ServiceResult Ok(string message = "", NotificationType type = NotificationType.Success)
        => new() { Success = true, Message = message, NotificationType = type };

    public static ServiceResult Fail(string message, string errorCode = "", NotificationType type = NotificationType.Error)
        => new() { Success = false, Message = message, ErrorCode = errorCode, NotificationType = type };
}

public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; set; }

    public static ServiceResult<T> Ok(T data, string message = "", NotificationType type = NotificationType.Success)
        => new() { Success = true, Data = data, Message = message, NotificationType = type };

    public new static ServiceResult<T> Fail(string message, string errorCode = "", NotificationType type = NotificationType.Error)
        => new() { Success = false, Message = message, ErrorCode = errorCode, NotificationType = type };
}

public static class ServiceResultExtensions
{
    public static ServiceResult<T> WithCorrelation<T>(this ServiceResult<T> result, string correlationId)
    {
        result.CorrelationId = correlationId;
        return result;
    }
}
