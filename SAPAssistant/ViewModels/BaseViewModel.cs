using System.Collections;
using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using SAPAssistant.Exceptions;
using SAPAssistant.Service;

namespace SAPAssistant.ViewModels;

public record ErrorOptions(
    string? Message = null,
    string? Title = null,
    NotificationType Severity = NotificationType.Error,
    object? AdditionalData = null);

public abstract partial class BaseViewModel : ObservableObject, INotifyDataErrorInfo
{
    protected readonly NotificationService NotificationService;
    protected readonly ILogger Logger;

    protected BaseViewModel(NotificationService notificationService, ILogger logger)
    {
        NotificationService = notificationService;
        Logger = logger;
    }

    [ObservableProperty]
    private bool isBusy;

    private readonly Dictionary<string, List<string>> _errors = new();

    public bool HasErrors => _errors.Any();

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public IEnumerable GetErrors(string? propertyName)
        => propertyName != null && _errors.TryGetValue(propertyName, out var errors)
            ? errors
            : Enumerable.Empty<string>();

    protected void AddError(string propertyName, string error)
    {
        if (!_errors.TryGetValue(propertyName, out var errors))
        {
            errors = new List<string>();
            _errors[propertyName] = errors;
        }

        if (!errors.Contains(error))
        {
            errors.Add(error);
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
    }

    protected void ClearErrors(string propertyName)
    {
        if (_errors.Remove(propertyName))
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
    }

    protected async Task<bool> ExecuteSafeAsync(Func<Task> action, Func<Exception, ErrorOptions>? optionsFactory = null)
    {
        try
        {
            await action();
            return true;
        }
        catch (Exception ex)
        {
            var options = optionsFactory?.Invoke(ex) ?? new ErrorOptions
            {
                Message = "❌ Ocurrió un error inesperado.",
                Title = "Error",
                Severity = NotificationType.Error,
                AdditionalData = null
            };

            Logger.LogError(ex, "{Title}: {Message} {@Data}", options.Title ?? "Error", options.Message, options.AdditionalData);
            NotificationService.Notify(ResultMessage.Fail(options.Message ?? "Error", type: options.Severity));
            return false;
        }
    }
}

