using Microsoft.AspNetCore.Mvc;
using PortalHorasApi.Model;
using QuatiTimeWebApi.Models;
using QuatiTimeWebApi.Services;

namespace QuatiTimeWebApi.Controllers;

[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly ChatParseService _parser;
    private readonly PortalSessionService _portalSession;
    private readonly TaskCacheService _taskCache;

    public ChatController(ChatParseService parser, PortalSessionService portalSession, TaskCacheService taskCache)
    {
        _parser = parser;
        _portalSession = portalSession;
        _taskCache = taskCache;
    }

    [HttpPost("parse")]
    public async Task<IActionResult> Parse([FromBody] ChatParseRequest request)
    {
        var payload = HttpContext.GetSession()!;

        if (!_taskCache.TryGet(payload.UserId, out var tasks))
        {
            var result = await _portalSession.WithRetry(payload, s => s.GetAllTasks());
            tasks = result.IsError ? new List<Tarefa>() : result.Value;
            if (!result.IsError) _taskCache.Set(payload.UserId, tasks);
        }

        var parsed = _parser.Parse(request.Message, tasks);
        return Ok(parsed);
    }
}
