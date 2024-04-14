namespace PongServer.Controllers.Models;

public class GameSession
{
    public string? GameCode { get; set; }

    public string? HostConnectionId { get; set; }

    public string? ClientConnectionId { get; set; }

}