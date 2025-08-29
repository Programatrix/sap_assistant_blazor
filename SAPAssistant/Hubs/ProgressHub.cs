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
        await Clients.Group(update.request_id).SendAsync("ProgressUpdate", update);
    }

    public override async Task OnConnectedAsync()
    {
        Console.WriteLine($"Cliente conectado: {Context.ConnectionId}");
        _ = Context.GetHttpContext()?.Request.Query["requestId"];
        await base.OnConnectedAsync();
    }
}
