using Microsoft.AspNetCore.SignalR;

namespace Projet_FullStack_FrontEnd.Hubs;

public class ChatHub : Hub
{
    public async Task SendMessage(string user, string message)
    {
        if (string.IsNullOrWhiteSpace(user))
            user = "Anonyme";

        if (string.IsNullOrWhiteSpace(message))
            return;

        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}