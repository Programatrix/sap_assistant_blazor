using System;
using System.Net.Http;
using System.Threading;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using SAPAssistant.Constants;
using SAPAssistant.Service;
using Xunit;

namespace SAPAssistant.Tests;

public class ConnectionServiceTests
{
    [Fact]
    public async Task GetConnectionsAsync_LogsError_OnHttpRequestException()
    {
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network"));

        var http = new HttpClient(handler.Object) { BaseAddress = new Uri("http://localhost") };
        var contextAccessor = new HttpContextAccessor();
        var ctx = new DefaultHttpContext();
        ctx.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("access_token", "token"),
            new Claim("remote_url", "remote"),
            new Claim(ClaimTypes.Name, "user")
        }, "test"));
        contextAccessor.HttpContext = ctx;
        var currentAccessor = new CurrentUserAccessor(contextAccessor);

        var logger = new Mock<ILogger<ConnectionService>>();
        var localizer = new Mock<IStringLocalizer<ErrorMessages>>();
        localizer.Setup(l => l[It.IsAny<string>()]).Returns<string>(s => new LocalizedString(s, s));

        var svc = new ConnectionService(http, logger.Object, localizer.Object, currentAccessor);

        await svc.GetConnectionsAsync();

        logger.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("obtener conexiones")),
                It.IsAny<HttpRequestException>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once);
    }
}
