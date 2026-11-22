using Microsoft.AspNetCore.Components.Server.Circuits;
using RealtorWeb.Components;
using RealtorWeb.Features.Interactions;
using RealtorWeb.Features.Properties;
using Refit;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();

var apiBaseAddress = builder.Configuration.GetValue<string>("ApiClients:RealtorApi:BaseAddress") ?? "http://localhost:5085/";

builder.Services.AddRefitClient<IPropertiesApi>(new RefitSettings(), "RealtorApi")
    .ConfigureHttpClient(client => client.BaseAddress = new Uri(apiBaseAddress, UriKind.Absolute));

builder.Services.AddScoped<InteractionContext>();
builder.Services.AddScoped<CircuitHandler, InteractionCircuitHandler>();
builder.Services.AddScoped<PropertyCatalogService>();
builder.Services.AddScoped<IPropertyCatalogService>(services =>
    new InteractionLoggingCatalogService(
        services.GetRequiredService<PropertyCatalogService>(),
        services.GetRequiredService<InteractionContext>(),
        services.GetRequiredService<ILogger<InteractionLoggingCatalogService>>()));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();
