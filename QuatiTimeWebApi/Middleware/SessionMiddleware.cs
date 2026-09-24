using QuatiTimeWebApi.Services;

namespace QuatiTimeWebApi.Middleware;

public static class SessionExtensions
{
    public static SessionPayload? GetSession(this HttpContext ctx)
        => ctx.Items["session"] as SessionPayload;

    public static void SetSession(this HttpContext ctx, SessionPayload payload)
        => ctx.Items["session"] = payload;
}

public class SessionMiddleware
{
    private readonly RequestDelegate _next;

    public SessionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext ctx, CookieEncryptionService encryption)
    {
        if (!ctx.Request.Path.StartsWithSegments("/api") || ctx.Request.Path.StartsWithSegments("/api/auth"))
        {
            await _next(ctx);
            return;
        }

        // Aceita token via header (dev/proxy) OU cookie (produção cross-origin)
        var token = ctx.Request.Headers["X-Session-Token"].FirstOrDefault()
                    ?? ctx.Request.Cookies["qts"];

        if (string.IsNullOrEmpty(token))
        {
            ctx.Response.StatusCode = 401;
            return;
        }

        var payload = encryption.Decrypt(token);
        if (payload is null)
        {
            ctx.Response.StatusCode = 401;
            return;
        }

        ctx.SetSession(payload);
        await _next(ctx);
    }
}
