using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using PongServer.Constants;
using PongServer.Controllers.Models;
using PongServer.Controllers.Models.Requests;
using PongServer.Hubs;

namespace PongServer.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class PongController : ControllerBase
{
    private readonly IHubContext<PongHub, IPongHub> _pongHub;
    private readonly IMemoryCache _memoryCache;

    public PongController(IHubContext<PongHub, IPongHub> pongHub, IMemoryCache memoryCache)
    {
        _pongHub = pongHub;
        _memoryCache = memoryCache;
    }

    [HttpPost]
    public async Task<IActionResult> HostGame([FromBody] HostGameRequest request)
    {
        string gameCode = Guid.NewGuid().ToString().Split("-")[0];

        UpdateGameSession(request.ConnectionId, gameCode, null);

        return Ok(gameCode);
    }

    [HttpPost]
    public async Task<IActionResult> JoinGame([FromBody] JoinGameRequest request)
    {
            GameSession gameSession = GetGameSession(request.GameCode);

        if (gameSession is null)
        {
            return NotFound("Oyun bulunamadi.");
        }

        if (gameSession.ClientConnectionId != null)
        {
            return BadRequest("Oyun dolu.");
        }

        UpdateGameSession(gameSession.HostConnectionId, request.GameCode, request.ConnectionId);

        await _pongHub.Clients.Client(gameSession.HostConnectionId).PlayerJoined(request.ConnectionId);
        await _pongHub.Clients.Client(request.ConnectionId).JoinedGame(gameSession.HostConnectionId);

        return Ok();
    }

    [HttpDelete]
    public async Task<IActionResult> LeaveGame([FromBody] string connectionId)
    {
        (GameSession gameSession, bool host) game = GetGameSessionByConnectionId(connectionId);

        if (game.gameSession is null)
        {
            return NoContent();
        }

        if (game.host)
        {
            DeleteGameSession(game.gameSession.GameCode!);
        }
        else
        {
            UpdateGameSession(game.gameSession.HostConnectionId!, game.gameSession.GameCode!, null);
        }

        return NoContent();
    }

    private GameSession GetGameSession(string gameCode)
    {
        List<GameSession> gameSessions = LoadGameSessions();

        return gameSessions.FirstOrDefault(s => s.GameCode == gameCode);
    }

    private (GameSession gameSession, bool host) GetGameSessionByConnectionId(string connectionId)
    {
        List<GameSession> gameSessions = LoadGameSessions();

        GameSession gameSession = gameSessions.FirstOrDefault(s => s.HostConnectionId == connectionId || s.ClientConnectionId == connectionId);

        bool host = gameSession!.HostConnectionId == connectionId;

        return (gameSession, host);
    }

    private void UpdateGameSession(string hostConnectionId, string gameCode, string clientConnectionId)
    {
        List<GameSession> gameSessions = LoadGameSessions();

        GameSession existingSession = gameSessions.FirstOrDefault(s => s.GameCode == gameCode);

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

        GameSession existingSession = gameSessions.ToList().FirstOrDefault(s => s.GameCode == gameCode);

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
        return !_memoryCache.TryGetValue(CacheKeys.GameSessions, out List<GameSession> gameSessions) ? [] : gameSessions;
    }

    private void SaveGameSessions(List<GameSession> gameSessions)
    {
        _memoryCache.Set(CacheKeys.GameSessions, gameSessions);
    }
}