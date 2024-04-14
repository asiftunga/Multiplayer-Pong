using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PongServer.Controllers.Models;
using PongServer.Hubs;

namespace PongServer.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PongController : ControllerBase
{
    private readonly IHubContext<PongHub> _pongHub;
    private readonly string _gameSessionsFilePath = Path.Combine(AppContext.BaseDirectory, "GameSessions.json");

    public PongController(IHubContext<PongHub> pongHub)
    {
        _pongHub = pongHub;
    }

    [HttpPost]
    public async Task<IActionResult> HostGame(string connectionId)
    {
        string gameCode = Guid.NewGuid().ToString();

        UpdateGameSession(connectionId, gameCode, null);

        return Ok(gameCode);
    }

    [HttpPost]
    public async Task<IActionResult> JoinGame(string connectionId, string gameCode)
    {
        GameSession? gameSession = GetGameSession(gameCode);

        if (gameSession is null)
        {
            return NotFound("Oyun bulunamadi.");
        }

        UpdateGameSession(gameSession.HostConnectionId, gameCode, connectionId);

        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> LeaveGame(string connectionId)
    {
        (GameSession? gameSession, bool host) tuple = GetGameSessionByConnectionId(connectionId);

        if (tuple.gameSession is null)
        {
            return NoContent();
        }

        if (tuple.host)
        {
            DeleteGameSession(tuple.gameSession.GameCode!);
        }
        else
        {
            UpdateGameSession(tuple.gameSession.HostConnectionId!, tuple.gameSession.GameCode!, null);
        }

        return Ok();
    }

    private GameSession? GetGameSession(string gameCode)
    {
        List<GameSession> gameSessions = LoadGameSessions();

        return gameSessions.FirstOrDefault(s => s.GameCode == gameCode);
    }

    private (GameSession? gameSession, bool host) GetGameSessionByConnectionId(string connectionId)
    {
        List<GameSession> gameSessions = LoadGameSessions();

        GameSession? gameSession = gameSessions.FirstOrDefault(s => s.HostConnectionId == connectionId || s.ClientConnectionId == connectionId);

        bool host = gameSession!.HostConnectionId == connectionId;

        return (gameSession, host);
    }

    private void UpdateGameSession(string? hostConnectionId, string? gameCode, string? clientConnectionId)
    {
        List<GameSession> gameSessions = LoadGameSessions();

        GameSession? existingSession = gameSessions.FirstOrDefault(s => s.GameCode == gameCode);

        if (existingSession is not null)
        {
            existingSession.GameCode = gameCode;

            existingSession.ClientConnectionId = clientConnectionId;
        }
        else
        {
            GameSession newSession = new () { GameCode = gameCode, HostConnectionId = hostConnectionId, ClientConnectionId = clientConnectionId };

            gameSessions.Add(newSession);
        }

        SaveGameSessions(gameSessions);
    }

    private void DeleteGameSession(string gameCode)
    {
        List<GameSession> gameSessions = LoadGameSessions();

        GameSession? existingSession = gameSessions.ToList().FirstOrDefault(s => s.GameCode == gameCode);

        if (existingSession is not null)
        {
            int index = gameSessions.ToList().IndexOf(existingSession);

            gameSessions.RemoveAt(index);
        }
        else
        {
            return;
        }

        SaveGameSessions(gameSessions);
    }

    private List<GameSession> LoadGameSessions()
    {
        if (!System.IO.File.Exists(_gameSessionsFilePath))
        {
            return new List<GameSession>();
        }

        string json = System.IO.File.ReadAllText(_gameSessionsFilePath);

        return JsonSerializer.Deserialize<List<GameSession>>(json) ?? new List<GameSession>();
    }


    private void SaveGameSessions(List<GameSession> gameSessions)
    {
        string json = JsonSerializer.Serialize(gameSessions);

        System.IO.File.WriteAllText(_gameSessionsFilePath, json);
    }
}