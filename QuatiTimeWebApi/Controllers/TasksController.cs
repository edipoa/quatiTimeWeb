using Microsoft.AspNetCore.Mvc;
using QuatiTimeWebApi.Middleware;
using QuatiTimeWebApi.Services;

namespace QuatiTimeWebApi.Controllers;

[ApiController]
[Route("api/tasks")]
public class TasksController : ControllerBase
{
    private readonly PortalSessionService _portalSession;
    private readonly TaskCacheService _cache;

    public TasksController(PortalSessionService portalSession, TaskCacheService cache)
    {
        _portalSession = portalSession;
        _cache = cache;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool refresh = false)
    {
        var payload = HttpContext.GetSession()!;

        if (!refresh && _cache.TryGet(payload.UserId, out var cached))
            return Ok(cached);

        var result = await _portalSession.WithRetry(payload, s => s.GetAllTasks());
        if (result.IsError)
            return StatusCode(502, new { error = string.Join(", ", result.Errors.Select(e => e.Description)) });

        _cache.Set(payload.UserId, result.Value);
        return Ok(result.Value);
    }
}
