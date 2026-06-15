using Microsoft.AspNetCore.Mvc;
using QuatiTimeWebApi.Middleware;
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
        // UserId derivado do username para ser estável entre sessões
        var userId = request.Username.ToLowerInvariant();
        var payload = new SessionPayload(request.Username, request.Password, userId);

        var ok = await _portalSession.CreateSession(payload);
        if (!ok)
            return Unauthorized(new { error = "Credenciais inválidas ou portal inacessível" });

        var cookieValue = _encryption.Encrypt(payload);
        var hours = _config.GetValue<int>("Cookie:ExpirationHours", 10);

        Response.Cookies.Append("qts", cookieValue, new CookieOptions
        {
            HttpOnly = true,
            Secure = false,   // false em dev local (sem HTTPS)
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddHours(hours)
        });

        // Retorna o token no body para o frontend usar como header (dev/proxy)
        // O cookie também é mantido como fallback para produção (cross-origin com HTTPS)
        return Ok(new { userId, token = cookieValue });
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
