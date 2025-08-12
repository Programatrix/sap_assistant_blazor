using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.DataProtection;
using SAPAssistant.Service;

namespace SAPAssistant.Tests;

public class TestJSRuntime : IJSRuntime
{
    private readonly ConcurrentDictionary<string, object?> _storage = new();

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
        => InvokeAsync<TValue>(identifier, CancellationToken.None, args);

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
    {
        if (identifier.Contains("get", StringComparison.OrdinalIgnoreCase))
        {
            var key = args![0]!.ToString()!;
            if (_storage.TryGetValue(key, out var value) && value is not null)
            {
                return ValueTask.FromResult((TValue)value);
            }
            return ValueTask.FromResult(default(TValue)!);
        }
        if (identifier.Contains("set", StringComparison.OrdinalIgnoreCase))
        {
            var key = args![0]!.ToString()!;
            _storage[key] = args![1];
        }
        if (identifier.Contains("delete", StringComparison.OrdinalIgnoreCase))
        {
            var key = args![0]!.ToString()!;
            _storage.TryRemove(key, out _);
        }
        return ValueTask.FromResult(default(TValue)!);
    }
}

public static class SessionContextFactory
{
    public static SessionContextService CreateWithDefaults(string? token = "token", string? userId = "user", string? remoteUrl = "remote")
    {
        var js = new TestJSRuntime();
        var protector = new EphemeralDataProtectionProvider().CreateProtector("Test");
        var storage = new ProtectedSessionStorage(js, protector);
        if (token != null) storage.SetAsync("token", token).GetAwaiter().GetResult();
        if (userId != null) storage.SetAsync("username", userId).GetAwaiter().GetResult();
        if (remoteUrl != null) storage.SetAsync("remote_url", remoteUrl).GetAwaiter().GetResult();
        return new SessionContextService(storage);
    }
}
