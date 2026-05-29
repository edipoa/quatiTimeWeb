using Microsoft.AspNetCore.Mvc;
using QuatiTimeWebApi.Models;
using QuatiTimeWebApi.Services;

namespace QuatiTimeWebApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly PortalSessionService _portalSession;
    private readonly CookieEncryptionService _encryption;
    private readonly IConfiguration _config;

    public AuthController(PortalSessionService portalSession, CookieEncryptionService encryption, IConfiguration config)
    {
        _portalSession = portalSession;
        _encryption = encryption;
        _config = config;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var userId = Guid.NewGuid().ToString();
        var payload = new SessionPayload(request.Username, request.Password, userId);

        var session = await _portalSession.CreateSession(payload);
        if (session.IsError)
            return Unauthorized(new { error = "Credenciais inválidas ou portal inacessível" });

        var cookieValue = _encryption.Encrypt(payload);
        var hours = _config.GetValue<int>("Cookie:ExpirationHours", 10);

        Response.Cookies.Append("qts", cookieValue, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTimeOffset.UtcNow.AddHours(hours)
        });

        return Ok(new { userId });
    }

    [HttpDelete("logout")]
    public IActionResult Logout()
    {
        var payload = HttpContext.GetSession();
        if (payload is not null)
            _portalSession.RemoveSession(payload.UserId);

        Response.Cookies.Delete("qts");
        return NoContent();
    }
}
