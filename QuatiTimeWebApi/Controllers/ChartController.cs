using ErrorOr;
using Microsoft.AspNetCore.Mvc;
using QuatiTimeWebApi.Middleware;
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
    public async Task<IActionResult> Get([FromQuery] DateOnly? startDate, [FromQuery] DateOnly? endDate)
    {
        var payload = HttpContext.GetSession()!;
        var result = await _portalSession.WithRetry(payload, s => s.GetChart(startDate, endDate));

        if (result.IsError)
        {
            var isAuthError = result.Errors.Any(e => e.Type == ErrorType.Unauthorized) ||
                              result.Errors.Any(e => e.Description is "A failure has occurred.");
            if (isAuthError)
                return Unauthorized(new { error = "Sessão do portal expirada. Faça login novamente." });

            return StatusCode(502, new { error = string.Join(", ", result.Errors.Select(e => e.Description)) });
        }

        return File(result.Value, "image/png");
    }
}
