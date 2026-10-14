using IntervalsMcp.Data;
using IntervalsMcp.Intervals;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Konfiguracja: appsettings.json + zmienne środowiskowe (np. Intervals__ApiKey)
builder.Services
    .AddOptions<IntervalsOptions>()
    .Bind(builder.Configuration.GetSection(IntervalsOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Typowany HttpClient — konfiguracja w IntervalsClient
builder.Services.AddHttpClient<IntervalsClient>();

// EF Core + PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// Serwer MCP po HTTP; narzędzia wykrywane po atrybutach [McpServerToolType]
builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

var app = builder.Build();

// Tworzy schemat bazy przy starcie (prosto do nauki; docelowo: migracje)
await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapMcp("/mcp");

app.Run();
