using RealtorWeb.Components;
using RealtorWeb.Features.Properties;
using Refit;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents();

var apiBaseAddress = builder.Configuration.GetValue<string>("ApiClients:RealtorApi:BaseAddress") ?? "http://localhost:5085/";

builder.Services.AddRefitClient<IPropertiesApi>(new RefitSettings(), "RealtorApi")
    .ConfigureHttpClient(client => client.BaseAddress = new Uri(apiBaseAddress, UriKind.Absolute));

builder.Services.AddScoped<IPropertyCatalogService, PropertyCatalogService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.MapStaticAssets();
app.MapRazorComponents<App>();

app.Run();
