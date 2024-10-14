namespace PongServer.Controllers.Models.Requests;

public class JoinGameRequest
{
    public string ConnectionId { get; set; }

    public string GameCode { get; set; }
}