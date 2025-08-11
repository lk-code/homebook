using Homebook.Backend.Core.Setup.Extensions;
using HomeBook.Backend.Endpoints;
using HomeBook.Backend.Extensions;
using Scalar.AspNetCore;
using Serilog;


var builder = WebApplication.CreateBuilder(args);
builder.Configuration.Sources.Clear();
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables(prefix: "HB_");

// Serilog einrichten
builder.Host.UseSerilog((ctx, services, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

builder.Services.AddOpenApi();

builder.Services.AddBackendServices(builder.Configuration)
    .AddBackendCoreSetup(builder.Configuration);

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    });
}

var app = builder.Build();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseCors();
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseDefaultFiles();
// app.UseStaticFiles();
app.MapFallbackToFile("index.html"); // <- important for Blazor Routing

app.MapVersionEndpoints()
    .MapSetupEndpoints();

app.Run();
