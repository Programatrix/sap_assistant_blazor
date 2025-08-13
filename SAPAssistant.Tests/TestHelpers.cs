using Microsoft.AspNetCore.Http;
using SAPAssistant.Service;
using System.Collections.Generic;

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
        return new SessionContextService(accessor);
    }
}
