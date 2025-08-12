using System.Text.Json.Serialization;
using SAPAssistant.Exceptions;

namespace SAPAssistant.Service.Shared.Transport;

// DTO de transporte (no sale de esta capa)
public class StdResponse<T>
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("data")]
    public T? Data { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("errorCode")]
    public string? ErrorCode { get; set; }

    [JsonPropertyName("traceId")]
    public string? TraceId { get; set; }

    [JsonPropertyName("notificationType")]
    public NotificationType? NotificationType { get; set; }
}
