using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using HomeBook.Frontend;
using HomeBook.Frontend.Extensions;
using HomeBook.Frontend.Services.Extensions;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
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

builder.Services.AddMudServices();
builder.Services.AddFrontendUiServices(builder.Configuration)
    .AddFrontendServices(builder.Configuration)
    .AddBackendClient(builder.Configuration);

await builder.Build().RunAsync();
