using System.Text.Json.Serialization;

namespace SAPAssistant.Models;

public class AssistantStartResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("data")]
    public AssistantStartData Data { get; set; } = new();

    [JsonPropertyName("traceId")]
    public string? TraceId { get; set; }

    [JsonPropertyName("error")]
    public object? Error { get; set; }
}

public class AssistantStartData
{
    [JsonPropertyName("tipo")]
    public string? Tipo { get; set; }

    [JsonPropertyName("mensaje")]
    public string? Mensaje { get; set; }

    [JsonPropertyName("output")]
    public string? Output { get; set; }

    [JsonPropertyName("request_id")]
    public string RequestId { get; set; } = string.Empty;
}