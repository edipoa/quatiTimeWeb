using Microsoft.AspNetCore.Mvc;
using QuatiTimeWebApi.Services;

namespace QuatiTimeWebApi.Controllers;

[ApiController]
[Route("api/chart")]
public class ChartController : ControllerBase
{
    private readonly PortalSessionService _portalSession;

    public ChartController(PortalSessionService portalSession)
    {
        _portalSession = portalSession;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var payload = HttpContext.GetSession()!;
        var result = await _portalSession.WithRetry(payload, s => s.GetChart());

        if (result.IsError)
            return StatusCode(502, new { error = string.Join(", ", result.Errors.Select(e => e.Description)) });

        return File(result.Value, "image/png");
    }
}
