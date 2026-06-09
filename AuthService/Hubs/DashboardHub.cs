using Microsoft.AspNetCore.SignalR;

namespace AuthService.Hubs;

public class DashboardHub : Hub
{
    public async Task JoinAdmin()
    {
        await Groups.AddToGroupAsync(
            Context.ConnectionId, "admins");
    }
}