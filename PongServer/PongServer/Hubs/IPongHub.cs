namespace PongServer.Hubs;

public interface IPongHub
{
    Task ClientCount(int count);

    Task PlayerJoined(string connectionId);

    Task JoinedGame(string hostConnectionId);

    Task  ReceiveOpponentPaddlePosition(double paddle);

    Task GameEnded();

    Task PlayerLeft();

    Task UpdateGameState();

    Task UpdatePlayerInput();

    Task StartGame();
}