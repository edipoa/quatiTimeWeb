using ErrorOr;
using Microsoft.AspNetCore.Mvc;
using PortalHorasApi.Model;
using QuatiTimeWebApi.Middleware;
using QuatiTimeWebApi.Models;
using QuatiTimeWebApi.Services;

namespace QuatiTimeWebApi.Controllers;

[ApiController]
[Route("api/records")]
public class RecordsController : ControllerBase
{
    private readonly RecordRepository _repo;
    private readonly PortalSessionService _portalSession;
    private readonly TaskCacheService _taskCache;

    public RecordsController(RecordRepository repo, PortalSessionService portalSession, TaskCacheService taskCache)
    {
        _repo = repo;
        _portalSession = portalSession;
        _taskCache = taskCache;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var payload = HttpContext.GetSession()!;

        if (!_taskCache.TryGet(payload.UserId, out var tasks))
        {
            var result = await _portalSession.WithRetry(payload, s => s.GetAllTasks());
            if (!result.IsError)
            {
                tasks = result.Value;
                _taskCache.Set(payload.UserId, tasks);
            }
        }

        var records = await _repo.GetAllAsync(payload.UserId, tasks ?? new List<Tarefa>());
        return Ok(records);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRecordRequest request)
    {
        var payload = HttpContext.GetSession()!;
        var id = await _repo.CreateAsync(payload.UserId, request);
        return Ok(new { id });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRecordRequest request)
    {
        var payload = HttpContext.GetSession()!;
        await _repo.UpdateAsync(payload.UserId, id, request);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var payload = HttpContext.GetSession()!;
        await _repo.DeleteAsync(payload.UserId, id);
        return NoContent();
    }

    [HttpPost("sync")]
    public async Task<IActionResult> Sync()
    {
        var payload = HttpContext.GetSession()!;
        var pending = await _repo.GetPendingAsync(payload.UserId);

        var results = new List<object>();
        foreach (var entry in pending)
        {
            var record = new Record
            {
                TaskId = entry.TaskId,
                Date = entry.Date,
                Description = entry.Description,
                Time = entry.Time,
            };

            var result = await _portalSession.WithRetry(payload, s => s.PostRecord(record));
            if (!result.IsError)
            {
                await _repo.MarkSynchronizedAsync(payload.UserId, entry.Id);
                results.Add(new { entry.Id, success = true });
            }
            else if (result.Errors.Any(e => e.Type == ErrorType.Unauthorized) ||
                     result.Errors.Any(e => e.Description is "A failure has occurred."))
            {
                return Unauthorized(new { error = "Sessão do portal expirada. Faça login novamente." });
            }
            else
            {
                results.Add(new { entry.Id, success = false, error = string.Join(", ", result.Errors.Select(e => e.Description)) });
            }
        }

        return Ok(results);
    }

    [HttpDelete("synced")]
    public async Task<IActionResult> DeleteSynced()
    {
        var payload = HttpContext.GetSession()!;
        await _repo.DeleteSynchronizedAsync(payload.UserId);
        return NoContent();
    }
}
