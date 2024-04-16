namespace PongServer.Hubs;

public interface IPongHub
{
    Task ClientCount(int count);

    Task PlayerJoined();

    Task GameEnded();

    Task PlayerLeft();

    Task UpdateGameState();

    Task UpdatePlayerInput();

    Task StartGame();
}