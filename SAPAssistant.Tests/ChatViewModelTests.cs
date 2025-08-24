using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using Moq;
using SAPAssistant;
using SAPAssistant.Service;
using SAPAssistant.Service.Interfaces;
using SAPAssistant.ViewModels;
using Xunit;

namespace SAPAssistant.Tests;

public class ChatViewModelTests
{
    [Fact]
    public async Task OnProgressEvent_SetsPhaseAndProgress()
    {
        var js = new Mock<IJSRuntime>();
        var assistant = new Mock<IAssistantService>();
        var history = new Mock<IChatHistoryService>();
        var state = new StateContainer();
        var notification = new Mock<INotificationService>();
        var localizer = new Mock<IStringLocalizer<ErrorMessages>>();
        var http = new HttpClient { BaseAddress = new Uri("http://localhost/api/v1/") };

        var vm = new ChatViewModel(js.Object, assistant.Object, history.Object, state, notification.Object, localizer.Object, http);

        await vm.OnProgressEvent("{\"phase\":\"processing\",\"progress\":0.5}");

        Assert.Equal("processing", vm.CurrentPhase);
        Assert.Equal(0.5, vm.ProgressValue);
    }
}
