using System;
using System.Collections;
using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SAPAssistant.Service;

namespace SAPAssistant.ViewModels;

public abstract partial class BaseViewModel : ObservableObject, INotifyDataErrorInfo
{
    protected readonly NotificationService NotificationService;
    
    protected BaseViewModel(NotificationService notificationService)
    {
        NotificationService = notificationService;
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

    protected async Task<bool> ExecuteSafeAsync(Func<Task> action, string context)
    {
        try
        {
            await action();
            return true;
        }
        catch (Exception ex)
        {
            NotificationService.NotifyException(ex, context);
            return false;
        }
    }
}

