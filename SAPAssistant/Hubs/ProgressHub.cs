using Microsoft.AspNetCore.SignalR;
using SAPAssistant.Models;

namespace SAPAssistant.Hubs;

public class ProgressHub : Hub
{
    public async Task SendProgress(ProgressUpdate update)
    {
        await Clients.Group(update.RequestId).SendAsync("ReceiveProgress", update);
    }

    public override async Task OnConnectedAsync()
    {
        var requestId = Context.GetHttpContext()?.Request.Query["requestId"];
        if (!string.IsNullOrEmpty(requestId))
            await Groups.AddToGroupAsync(Context.ConnectionId, requestId);

        await base.OnConnectedAsync();
    }
}
