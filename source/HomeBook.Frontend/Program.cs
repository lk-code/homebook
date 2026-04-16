using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using HomeBook.Frontend;
using HomeBook.Frontend.Extensions;
using HomeBook.Frontend.ModuleCore;
using HomeBook.Frontend.Services.Extensions;
using MudBlazor.Services;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
{
    {
        "Frontend:Host", builder.HostEnvironment.BaseAddress
    }
});

builder.Services.AddScoped(sp =>
{
    IConfiguration configuration = sp.GetRequiredService<IConfiguration>();
    string? webAddress = configuration["Frontend:Host"];

    if (string.IsNullOrEmpty(webAddress))
        throw new ArgumentNullException($"Frontend:Host is not configured");

    return new HttpClient
    {
        BaseAddress = new Uri(webAddress)
    };
});

builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = MudBlazor.Defaults.Classes.Position.TopRight;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.NewestOnTop = true;
    config.SnackbarConfiguration.SnackbarVariant = MudBlazor.Variant.Filled;

    config.SnackbarConfiguration.VisibleStateDuration = 10000;
    config.SnackbarConfiguration.HideTransitionDuration = 500;
    config.SnackbarConfiguration.ShowTransitionDuration = 500;
});
builder.Services.AddFrontendUiServices(builder.Configuration)
    .AddFrontendServices(builder.Configuration)
    .AddBackendClient(builder.Configuration);

builder.AddModules(
    builder.HomeBook(),
    (moduleBuilder) =>
    {
        // app modules
        moduleBuilder
            .AddModule<HomeBook.Frontend.Module.Finances.Module>()
            .AddModule<HomeBook.Frontend.Module.Kitchen.Module>()
            .AddModule<HomeBook.Frontend.Module.PlatformInfo.Module>();
    });

// build app
builder.BuildHomeBook();
WebAssemblyHost app = builder.Build();

// run app
await app.RunModulesPostBuild();

await app.RunAsync();
