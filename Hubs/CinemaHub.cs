using Microsoft.AspNetCore.SignalR;

namespace CinemaManagement.Hubs;

public class CinemaHub : Hub
{
    public async Task NotifyCinemaChanged(string action, object data)
    {
        await Clients.All.SendAsync("CinemaChanged", action, data);
    }
}