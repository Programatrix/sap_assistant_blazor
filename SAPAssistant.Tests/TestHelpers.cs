using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.JSInterop;
using SAPAssistant.Service;
using System.Collections.Generic;
using Moq;

namespace SAPAssistant.Tests;

#nullable enable
public static class SessionContextFactory
{
    public static SessionContextService CreateWithDefaults(string? token = "token", string? userId = "user", string? remoteUrl = "remote")
    {
        var context = new DefaultHttpContext();
        var cookies = new List<string>();
        if (token != null) cookies.Add($"token={token}");
        if (userId != null) cookies.Add($"username={userId}");
        if (remoteUrl != null) cookies.Add($"remote_url={remoteUrl}");
        context.Request.Headers["Cookie"] = string.Join("; ", cookies);
        var accessor = new HttpContextAccessor { HttpContext = context };
        var js = new Mock<IJSRuntime>();
        var provider = new EphemeralDataProtectionProvider();
        var storage = new ProtectedSessionStorage(js.Object, provider);
        return new SessionContextService(accessor, storage);
    }
}
