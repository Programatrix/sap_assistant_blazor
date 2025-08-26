using Microsoft.AspNetCore.SignalR;
using SAPAssistant.Models;

namespace SAPAssistant.Hubs;

public class ProgressHub : Hub
{
    public async Task Subscribe(string requestId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, requestId);
    }

    public async Task SendProgress(ProgressUpdate update)
    {
        await Clients.Group(update.RequestId).SendAsync("ProgressUpdate", update);
    }

    public override async Task OnConnectedAsync()
    {
        _ = Context.GetHttpContext()?.Request.Query["requestId"];
        await base.OnConnectedAsync();
    }
}
