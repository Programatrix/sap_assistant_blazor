namespace SAPAssistant.Service
{
    public class ErrorService
    {
        public event Action<string>? OnError;

        private readonly ILogger<ErrorService> _logger;

        public ErrorService(ILogger<ErrorService> logger)
        {
            _logger = logger;
        }

        public void NotifyError(Exception ex, string context = "")
        {
            _logger.LogError(ex, "Error en {Context}", context);
            OnError?.Invoke($"⚠️ {context}: {ex.Message}");
        }
    }

}
