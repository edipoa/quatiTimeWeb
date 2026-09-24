using QuatiTimeWebApi.Data;
using QuatiTimeWebApi.Middleware;
using QuatiTimeWebApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

// PortalHorasApi — qualificado para evitar colisão com ASP.NET Core
builder.Services.AddSingleton<PortalHorasApi.IApiConfiguration, PortalHorasApi.DefaultApiConfiguration>();
builder.Services.AddTransient<PortalHorasApi.IHttpContextFactory, PortalHorasApi.DefaultHttpContextFactory>();
builder.Services.AddTransient<PortalHorasApi.Client>();

builder.Services.AddSingleton<PortalSessionService>();
builder.Services.AddSingleton<TaskCacheService>();
builder.Services.AddSingleton<CookieEncryptionService>();
builder.Services.AddSingleton<Database>();
builder.Services.AddScoped<RecordRepository>();
builder.Services.AddScoped<ChatParseService>();

var app = builder.Build();

var db = app.Services.GetRequiredService<Database>();
app.Logger.LogInformation("SQLite database: {Path}", db.FilePath);
await db.InitializeAsync();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors();
app.UseMiddleware<SessionMiddleware>();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
