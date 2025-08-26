namespace SAPAssistant.Models;

public record ProgressUpdate(string RequestId, string Phase, int Percent, string Message);
