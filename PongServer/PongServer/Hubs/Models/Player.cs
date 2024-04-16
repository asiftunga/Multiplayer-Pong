namespace PongServer.Hubs.Models;

public class Player
{
    public string ConnectionId { get; set; }

    public bool IsHost { get; set; }

    public string? GameCode { get; set; }
}