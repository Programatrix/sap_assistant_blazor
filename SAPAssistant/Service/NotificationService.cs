using SAPAssistant.Exceptions;

public class NotificationService
{
    public event Action<ResultMessage>? OnNotify;

    public void ShowToast(ResultMessage result)
    {
        OnNotify?.Invoke(result);
    }
}
