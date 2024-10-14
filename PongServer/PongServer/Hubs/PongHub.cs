using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using PongServer.Constants;
using PongServer.Controllers.Models;
using PongServer.Hubs.Models;

namespace PongServer.Hubs;

public class PongHub : Hub<IPongHub>
{
    private readonly List<Player> _players = [];
    private readonly IMemoryCache _memoryCache;
    private static int ConnectedClientCount = 0;

    public PongHub(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public override async Task OnConnectedAsync()
    {
        if (_players.All(x => x.ConnectionId != Context.ConnectionId))
        {
            ConnectedClientCount++;
        }

        await Clients.All.ClientCount(ConnectedClientCount);
    }

    //todo : methodlar refactor edilmeli ve kurgu tekrar gozden gecirilmeli
    public override async Task OnDisconnectedAsync(Exception exception)
    {

    }

    public async Task UpdatePaddlePosition(double paddleY)
    {
        Console.WriteLine($"paddle pozisyonu : {paddleY}");

        var (gameSession, isHost) = GetGameSessionByConnectionId(Context.ConnectionId);

        if (gameSession != null)
        {
            string opponentConnectionId = isHost ? gameSession.ClientConnectionId : gameSession.HostConnectionId;

            if (!string.IsNullOrEmpty(opponentConnectionId))
            {
                await Clients.Client(opponentConnectionId).ReceiveOpponentPaddlePosition(paddleY);
            }
        }
    }

    public async Task StartGame()
    {
        var (gameSession, isHost) = GetGameSessionByConnectionId(Context.ConnectionId);

        if (gameSession != null && isHost)
        {

        }
    }

    public void SendPlayerInput(string direction)
    {

    }

    public void UpdateGameState(Ball ball, Paddle hostPaddle, Paddle opponentPaddle)
    {

    }

    private (GameSession gameSession, bool isHost) GetGameSessionByConnectionId(string connectionId)
    {
        List<GameSession> gameSessions = LoadGameSessions();

        var gameSession = gameSessions.FirstOrDefault(s => s.HostConnectionId == connectionId || s.ClientConnectionId == connectionId);

        bool isHost = gameSession?.HostConnectionId == connectionId;

        return (gameSession, isHost);
    }

    private List<GameSession> LoadGameSessions()
    {
        return _memoryCache.TryGetValue(CacheKeys.GameSessions, out List<GameSession> gameSessions) ? gameSessions : [];
    }
}