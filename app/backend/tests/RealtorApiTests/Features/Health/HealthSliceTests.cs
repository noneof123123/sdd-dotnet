using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using RealtorApi.Features.Health;
using RealtorApi.Infrastructure.Endpoints;

namespace RealtorApiTests.Features.Health;

public sealed class HealthSliceTests
{
    [Fact]
    public async Task HealthSlice_MapsGetHealthEndpoint()
    {
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        ISlice slice = new HealthSlice();
        slice.AddEndpoint(app);

        var dataSourcesProperty = typeof(WebApplication).GetProperty(
            "DataSources",
            BindingFlags.Instance | BindingFlags.NonPublic);
        var dataSources = Assert.IsAssignableFrom<ICollection<EndpointDataSource>>(
            dataSourcesProperty!.GetValue(app));

        var endpoint = dataSources
            .SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>()
            .Single(candidate => candidate.RoutePattern.RawText == "/health");

        Assert.Equal("GET", endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods.Single());

        var context = new DefaultHttpContext();
        context.RequestServices = app.Services;
        context.Response.Body = new MemoryStream();
        await endpoint.RequestDelegate!(context);

        context.Response.Body.Position = 0;
        using var document = await JsonDocument.ParseAsync(context.Response.Body);
        Assert.Equal("healthy", document.RootElement.GetProperty("status").GetString());
    }
}
