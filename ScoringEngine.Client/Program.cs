using Microsoft.Extensions.Hosting;
using ScoringEngine.Client.Scoring;
using ScoringEngine.Client.Scoring.ScriptingBackends;
using ScoringEngine.Client.Services;
using ScoringEngine.Services;

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

if (OperatingSystem.IsLinux())
{
    builder.Services.AddTransient<IScriptBackend, LinuxScriptBackend>();
}
else
{
    builder.Services.AddTransient<IScriptBackend, WindowsScriptBackend>();
}

// Add services to the container.
builder.Services.AddSingleton<ConfigurationService, ConfigurationService>();
builder.Services.AddSingleton<ConnectionService, ConnectionService>();
builder.Services.AddSingleton<ScoringService, ScoringService>();
builder.Services.AddRazorPages();
builder.Services.AddHostedService<ScoreCheckService>();

builder.Logging.AddFile("Logs/info");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
