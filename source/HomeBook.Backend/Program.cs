using HomeBook.Backend;
using HomeBook.Backend.Abstractions;
using HomeBook.Backend.Endpoints;
using HomeBook.Backend.EnvironmentHandler;
using HomeBook.Backend.Extensions;
using HomeBook.Backend.Core.Account.Extensions;
using HomeBook.Backend.Middleware;
using HomeBook.Backend.ModuleCore;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using Serilog;
using System.Text.Json.Serialization;

#if DEBUG
string developmentEnvFile = Path.Combine("env", "Development.env");
EnvironmentLoader.LoadEnvFile(developmentEnvFile);
#endif

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Configuration.Sources.Clear();
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddJsonFile(PathHandler.RuntimeConfigurationFilePath, optional: true, reloadOnChange: true)
    .AddEnvironmentVariables(prefix: "HB_");

// Serilog einrichten
builder.Host.UseSerilog((ctx, services, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

builder.Services.AddOpenApi(options =>
{
    options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_0;
});
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict;
});

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

InstanceStatus instanceStatus = builder.Configuration.GetCurrentInstanceStatus();

switch (instanceStatus)
{
    // map endpoints that are only available in setup mode
    case InstanceStatus.SETUP:
        builder.Services.AddDependenciesForSetup(builder.Configuration,
            instanceStatus);
        break;
    // map endpoints that are only available in running mode
    case InstanceStatus.RUNNING:
        builder.Services.AddDependenciesForRuntime(builder.Configuration,
            instanceStatus);
        break;
}

builder.Services.AddJwtAuthentication(builder.Configuration, instanceStatus);

if (builder.Environment.IsDevelopment())
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    });

if (instanceStatus == InstanceStatus.RUNNING)
    builder.AddModules(
        builder.HomeBook(),
        (moduleBuilder) =>
        {
            // app modules
            moduleBuilder
                .AddModule<HomeBook.Backend.Module.Finances.Module>()
                .AddModule<HomeBook.Backend.Module.Kitchen.Module>();
        });

WebApplication app = builder.Build();

Log.Information("HomeBook Backend application starting up - Version: {Version}",
    app.Configuration["Version"] ?? "Unknown");

app.UseSerilogRequestLogging();

if (instanceStatus == InstanceStatus.RUNNING)
{
    app.UseAuthentication();
    app.UseMiddleware<AdminAuthorizationMiddleware>();
    app.UseAuthorization();
}

if (app.Environment.IsDevelopment())
{
    app.UseCors();
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseDefaultFiles();

#region map endpoints

// map endpoints that are always available
app.MapVersionEndpoints()
    .MapSystemEndpoints()
    .MapPlatformEndpoints();

switch (instanceStatus)
{
    // map endpoints that are only available in setup mode
    case InstanceStatus.SETUP:
        app.MapSetupEndpoints();
        break;
    // map endpoints that are only available in running mode
    case InstanceStatus.RUNNING:
        app.MapSetupEndpoints()
            .MapUpdateEndpoints()
            .MapAccountEndpoints()
            .MapInfoEndpoints()
            .MapUserEndpoints()
            .MapSearchEndpoints()
            .MapStorageFileEndpoints()
            .MapStorageScopeEndpoints()
            .MapMediaEndpoints();
        break;
}

#endregion

if (instanceStatus == InstanceStatus.RUNNING)
    await app.RunModulesPostBuild();

app.Run();
