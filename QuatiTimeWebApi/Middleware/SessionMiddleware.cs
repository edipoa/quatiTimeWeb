using QuatiTimeWebApi.Services;

namespace QuatiTimeWebApi.Middleware;

public static class SessionExtensions
{
    private const string CookieName = "qts";

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
        if (ctx.Request.Path.StartsWithSegments("/api/auth"))
        {
            await _next(ctx);
            return;
        }

        var cookie = ctx.Request.Cookies["qts"];
        if (string.IsNullOrEmpty(cookie))
        {
            ctx.Response.StatusCode = 401;
            return;
        }

        var payload = encryption.Decrypt(cookie);
        if (payload is null)
        {
            ctx.Response.StatusCode = 401;
            return;
        }

        ctx.SetSession(payload);
        await _next(ctx);
    }
}
