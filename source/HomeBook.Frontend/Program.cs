using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using HomeBook.Frontend;
using HomeBook.Frontend.Services.Extensions;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
{
    { "Frontend:Host", builder.HostEnvironment.BaseAddress },
    { "Backend:ApiUrl", "https://localhost:7000" } // Backend API URL
});

// HttpClient für API-Aufrufe mit Authentifizierung
builder.Services.AddHttpClient("Backend", client =>
{
    client.BaseAddress = new Uri("https://localhost:7000");
})
.AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>()
    .CreateClient("Backend"));

// Standard HttpClient für lokale Ressourcen
builder.Services.AddScoped(sp =>
{
    IConfiguration configuration = sp.GetRequiredService<IConfiguration>();
    string? webAddress = configuration["Frontend:Host"];

    if (string.IsNullOrEmpty(webAddress))
        throw new ArgumentNullException($"Frontend:Host is not configured");

    return new HttpClient { BaseAddress = new Uri(webAddress) };
});

// Authentifizierung konfigurieren
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddSingleton<AuthenticationStateProvider, PersistentAuthenticationStateProvider>();

builder.Services.AddMudServices();
builder.Services.AddFrontendServices(builder.Configuration)
    .AddBackendClient(builder.Configuration);

await builder.Build().RunAsync();
