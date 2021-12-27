using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;

using ScoringEngine.Server.Data;
using ScoringEngine.Server.Services;

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ScoringEngineDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ScoringEngineServerContext")));
builder.Services.AddRazorPages();
builder.Services.AddGrpc();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.WebHost.UseKestrel(so =>
{
    so.Listen(IPAddress.Any, 5001, options =>
    {
        options.Protocols = HttpProtocols.Http2;
    });
    so.Listen(IPAddress.Any, 5000);
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ScoringEngineDbContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred creating the DB.");

        throw;
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/CommsError");
}
else
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapGrpcService<ScoringService>();
app.MapGrpcService<SessionService>();
app.MapRazorPages();

app.Run();
