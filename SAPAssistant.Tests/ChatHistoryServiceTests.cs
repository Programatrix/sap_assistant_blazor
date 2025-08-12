using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using SAPAssistant.Constants;
using SAPAssistant.Service;
using Xunit;

namespace SAPAssistant.Tests;

public class ChatHistoryServiceTests
{
    [Fact]
    public async Task GetChatSessionAsync_LogsError_OnHttpRequestException()
    {
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network"));

        var http = new HttpClient(handler.Object) { BaseAddress = new Uri("http://localhost") };
        var session = SessionContextFactory.CreateWithDefaults();

        var logger = new Mock<ILogger<ChatHistoryService>>();
        var localizer = new Mock<IStringLocalizer<ErrorMessages>>();
        localizer.Setup(l => l[It.IsAny<string>()]).Returns<string>(s => new LocalizedString(s, s));

        var svc = new ChatHistoryService(http, session, logger.Object, localizer.Object);

        await svc.GetChatSessionAsync("chat1");

        logger.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("obtener el chat")),
                It.IsAny<HttpRequestException>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once);
    }
}
