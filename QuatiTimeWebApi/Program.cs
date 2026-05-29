using PortalHorasApi;
using QuatiTimeWebApi.Data;
using QuatiTimeWebApi.Middleware;
using QuatiTimeWebApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// CORS — allow Vercel frontend
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

// PortalHorasApi
builder.Services.AddSingleton<IApiConfiguration, DefaultApiConfiguration>();
builder.Services.AddTransient<IHttpContextFactory, DefaultHttpContextFactory>();
builder.Services.AddTransient<Client>();

// App services
builder.Services.AddSingleton<PortalSessionService>();
builder.Services.AddSingleton<TaskCacheService>();
builder.Services.AddSingleton<CookieEncryptionService>();
builder.Services.AddSingleton<Database>();
builder.Services.AddScoped<RecordRepository>();
builder.Services.AddScoped<ChatParseService>();

var app = builder.Build();

// Initialize DB
await app.Services.GetRequiredService<Database>().InitializeAsync();

app.UseCors();
app.UseMiddleware<SessionMiddleware>();
app.MapControllers();

app.Run();
