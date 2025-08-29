namespace SAPAssistant.Models;

public record ProgressUpdate(string request_id, string Phase, int Percent, string Message);
