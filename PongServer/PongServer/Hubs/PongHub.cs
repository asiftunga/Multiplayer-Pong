using Microsoft.AspNetCore.SignalR;
using PongServer.Hubs.Models;

namespace PongServer.Hubs;

public class PongHub : Hub<IPongHub>
{
    private readonly List<Player> players = new();
    private static int ConnectedClientCount = 0;

    public override async Task OnConnectedAsync()
    {
        if (!players.Any(x => x.ConnectionId == Context.ConnectionId))
        {
            ConnectedClientCount++;
        }

        await Clients.All.ClientCount(ConnectedClientCount);
    }

    //todo : methodlar refactor edilmeli ve kurgu tekrar gozden gecirilmeli
    public override async Task OnDisconnectedAsync(Exception exception)
    {

    }

    public void SendPlayerInput(string direction)
    {

    }

    public void UpdateGameState(Ball ball, Paddle hostPaddle, Paddle opponentPaddle)
    {

    }
}