using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using CVTech.Web;
using CVTech.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// HttpClient pointé sur l'origine qui héberge le front (l'API elle-même).
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<ApiClient>();
builder.Services.AddScoped<SessionUtilisateur>();
builder.Services.AddScoped<ServiceNotifications>();

await builder.Build().RunAsync();
