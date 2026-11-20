using System.Reflection;
using System.Text;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using RealtorApi.Domain.Properties;
using RealtorApi.Features.Health;
using RealtorApi.Features.Properties.ChangePropertyStatus;
using RealtorApi.Features.Properties.CreateProperty;
using RealtorApi.Features.Properties.GetPropertyById;
using RealtorApi.Features.Properties.ListProperties;
using RealtorApi.Features.Properties.UpdateProperty;
using RealtorApi.Infrastructure.Endpoints;
using RealtorApi.Infrastructure.Handlers;
using RealtorApi.Infrastructure.OpenApi;
using RealtorApi.Infrastructure.Persistence;
using RealtorApi.Infrastructure.Storage;

namespace RealtorApiTests.Features.Properties.ChangePropertyStatus;

public sealed class ChangePropertyStatusSliceTests
{
    private static readonly Guid ExistingId = Guid.Parse("3f8b1c2d-4e5a-4b6c-8d9e-0a1b2c3d4e5f");

    [Fact]
    public void SliceHandlerAndValidator_ArePublicConcreteAndDiscoverable()
    {
        Assert.True(typeof(ISlice).IsAssignableFrom(typeof(ChangePropertyStatusSlice)));
        Assert.False(typeof(ChangePropertyStatusSlice).IsAbstract);
        Assert.True(typeof(IHandler).IsAssignableFrom(typeof(ChangePropertyStatusHandler)));
        Assert.True(typeof(IValidator<ChangePropertyStatusRequest>).IsAssignableFrom(typeof(ChangePropertyStatusValidator)));
    }

    [Fact]
    public void Handler_IsRegisteredByAssemblyScanning()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.AddSingleton<IPropertyImageStorage, StubPropertyImageStorage>();
        services.RegisterHandlers(typeof(ChangePropertyStatusHandler).Assembly);

        using var provider = services.BuildServiceProvider();

        Assert.Contains(provider.GetServices<IHandler>(), handler => handler is ChangePropertyStatusHandler);
    }

    [Fact]
    public void Slices_AreRegisteredByAssemblyScanningWithoutAffectingExistingSlices()
    {
        var services = new ServiceCollection();
        services.RegisterSlices(typeof(ChangePropertyStatusSlice).Assembly);

        using var provider = services.BuildServiceProvider();
        var slices = provider.GetServices<ISlice>().ToArray();

        Assert.Contains(slices, slice => slice is ChangePropertyStatusSlice);
        Assert.Contains(slices, slice => slice is GetPropertyByIdSlice);
        Assert.Contains(slices, slice => slice is CreatePropertySlice);
        Assert.Contains(slices, slice => slice is UpdatePropertySlice);
        Assert.Contains(slices, slice => slice is ListPropertiesSlice);
        Assert.Contains(slices, slice => slice is HealthSlice);
    }

    [Fact]
    public void AddEndpoint_MapsPatchPropertyStatus()
    {
        var app = BuildApp();

        var endpoint = FindChangeStatusEndpoint(app);

        Assert.Equal("PATCH", endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods.Single());
        Assert.Equal("/api/properties/{id}/status", endpoint.RoutePattern.RawText);
        Assert.Equal(
            RealtorOpenApiConstants.ChangePropertyStatusOperationId,
            endpoint.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName);
    }

    [Fact]
    public async Task PatchWithDifferentStatus_ReturnsOkWithFullRepresentation()
    {
        var app = BuildApp();
        var endpoint = FindChangeStatusEndpoint(app);
        var context = CreateHttpContext(app, ExistingId, """{"status":"Rented"}""");
        var before = await ReadStoredPropertyAsync(app, ExistingId);

        await endpoint.RequestDelegate!(context);

        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        using var document = await ReadJsonAsync(context);
        var root = document.RootElement;
        Assert.Equal(ExistingId.ToString(), root.GetProperty("id").GetString());
        Assert.Equal("Casa con imagen", root.GetProperty("title").GetString());
        Assert.Equal("Descripción con imagen.", root.GetProperty("description").GetString());
        Assert.Equal("Avenida Central 456", root.GetProperty("address").GetString());
        Assert.Equal(320000m, root.GetProperty("price").GetDecimal());
        Assert.Equal("Rented", root.GetProperty("status").GetString());
        Assert.Equal(4, root.GetProperty("bedroomCount").GetInt32());
        Assert.Equal(3, root.GetProperty("bathroomCount").GetInt32());
        Assert.Equal(180m, root.GetProperty("areaSquareMeters").GetDecimal());
        Assert.Equal(
            "http://localhost:5023/assets/properties/photo.png",
            root.GetProperty("imageUrl").GetString());
        Assert.Equal("2026-01-01T10:00:00Z", root.GetProperty("createdAt").GetString());
        Assert.True(root.GetProperty("updatedAt").GetString() != before.UpdatedAt?.ToString("O"));
        Assert.False(root.TryGetProperty("items", out _));
        Assert.False(root.TryGetProperty("totalItems", out _));

        var after = await ReadStoredPropertyAsync(app, ExistingId);
        Assert.Equal(PropertyStatus.Rented, after.Status);
        Assert.Equal(before.Title, after.Title);
        Assert.Equal(before.ImageUrl, after.ImageUrl);
        Assert.Equal(before.CreatedAt, after.CreatedAt);
        Assert.NotEqual(before.UpdatedAt, after.UpdatedAt);
    }

    [Fact]
    public async Task PatchWithExistingStatus_ReturnsOkWithoutChangingUpdatedAt()
    {
        var app = BuildApp();
        var endpoint = FindChangeStatusEndpoint(app);
        var before = await ReadStoredPropertyAsync(app, ExistingId);
        var context = CreateHttpContext(app, ExistingId, """{"status":"Available"}""");

        await endpoint.RequestDelegate!(context);

        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        using var document = await ReadJsonAsync(context);
        Assert.Equal("Available", document.RootElement.GetProperty("status").GetString());
        Assert.Equal("2026-02-01T10:00:00Z", document.RootElement.GetProperty("updatedAt").GetString());
        var after = await ReadStoredPropertyAsync(app, ExistingId);
        Assert.Equal(before.Status, after.Status);
        Assert.Equal(before.UpdatedAt, after.UpdatedAt);
    }

    [Fact]
    public async Task PatchWithMalformedId_ReturnsBadRequestWithoutChanges()
    {
        var app = BuildApp();
        var endpoint = FindChangeStatusEndpoint(app);
        var before = await ReadStoredPropertyAsync(app, ExistingId);
        var context = CreateHttpContext(app, ExistingId, """{"status":"Rented"}""", rawId: "no-es-un-guid");

        await endpoint.RequestDelegate!(context);

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        using var document = await ReadJsonAsync(context);
        Assert.True(document.RootElement.TryGetProperty("errors", out var errors));
        Assert.True(errors.TryGetProperty("Id", out _));
        AssertUnchanged(before, await ReadStoredPropertyAsync(app, ExistingId));
    }

    [Fact]
    public async Task PatchWithUnknownId_ReturnsNotFoundWithoutChanges()
    {
        var app = BuildApp();
        var endpoint = FindChangeStatusEndpoint(app);
        var before = await ReadStoredPropertyAsync(app, ExistingId);
        var context = CreateHttpContext(app, Guid.NewGuid(), """{"status":"Rented"}""");

        await endpoint.RequestDelegate!(context);

        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
        using var document = await ReadJsonAsync(context);
        Assert.Equal(404, document.RootElement.GetProperty("status").GetInt32());
        AssertUnchanged(before, await ReadStoredPropertyAsync(app, ExistingId));
    }

    [Fact]
    public async Task PatchWithEmptyBody_ReturnsBadRequestWithoutChanges()
    {
        var app = BuildApp();
        var endpoint = FindChangeStatusEndpoint(app);
        var before = await ReadStoredPropertyAsync(app, ExistingId);
        var context = CreateHttpContext(app, ExistingId, string.Empty);

        await endpoint.RequestDelegate!(context);

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        AssertUnchanged(before, await ReadStoredPropertyAsync(app, ExistingId));
    }

    [Theory]
    [InlineData("""{}""")]
    [InlineData("""{"status":null}""")]
    [InlineData("""{"status":""}""")]
    [InlineData("""{"status":"   "}""")]
    [InlineData("""{"status":"Unknown"}""")]
    [InlineData("""{"status":"rented"}""")]
    public async Task PatchWithInvalidStatus_ReturnsValidationProblemWithStatusKey(string json)
    {
        var app = BuildApp();
        var endpoint = FindChangeStatusEndpoint(app);
        var before = await ReadStoredPropertyAsync(app, ExistingId);
        var context = CreateHttpContext(app, ExistingId, json);

        await endpoint.RequestDelegate!(context);

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        using var document = await ReadJsonAsync(context);
        Assert.True(document.RootElement.TryGetProperty("errors", out var errors));
        Assert.True(errors.TryGetProperty("Status", out _));
        AssertUnchanged(before, await ReadStoredPropertyAsync(app, ExistingId));
    }

    [Fact]
    public async Task PatchWithExistingProperty_DoesNotExposePhysicalPaths()
    {
        var app = BuildApp();
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var property = await dbContext.Properties.SingleAsync(candidate => candidate.Id == ExistingId);
            property.ImageUrl = "/var/www/private/photo.png";
            await dbContext.SaveChangesAsync();
        }

        var endpoint = FindChangeStatusEndpoint(app);
        var context = CreateHttpContext(app, ExistingId, """{"status":"Maintenance"}""");

        await endpoint.RequestDelegate!(context);

        var body = await ReadBodyAsync(context);
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        Assert.DoesNotContain("/var/www", body, StringComparison.Ordinal);
        Assert.DoesNotContain("wwwroot", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(app.Environment.ContentRootPath, body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task PatchWhenHandlerFails_ReturnsGenericInternalProblemWithoutChanges()
    {
        var app = BuildApp(useFailingHandler: true);
        var endpoint = FindChangeStatusEndpoint(app);
        var before = await ReadStoredPropertyAsync(app, ExistingId);
        var context = CreateHttpContext(app, ExistingId, """{"status":"Rented"}""");

        await endpoint.RequestDelegate!(context);

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        var body = await ReadBodyAsync(context);
        Assert.DoesNotContain("ObjectDisposed", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", body, StringComparison.OrdinalIgnoreCase);
        AssertUnchanged(before, await ReadStoredPropertyAsync(app, ExistingId));
    }

    private static WebApplication BuildApp(bool useFailingHandler = false)
    {
        var builder = WebApplication.CreateBuilder();
        var databaseName = Guid.NewGuid().ToString();
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(databaseName));
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddValidatorsFromAssembly(typeof(ChangePropertyStatusValidator).Assembly, ServiceLifetime.Scoped);

        if (useFailingHandler)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;
            var failingContext = new AppDbContext(options);
            failingContext.Dispose();
            builder.Services.AddSingleton(
                new ChangePropertyStatusHandler(failingContext, NullLogger<ChangePropertyStatusHandler>.Instance));
        }

        builder.Services.RegisterSlices(typeof(ChangePropertyStatusSlice).Assembly);
        builder.Services.RegisterHandlers(typeof(ChangePropertyStatusHandler).Assembly);

        var app = builder.Build();
        SeedProperties(app);
        app.MapSliceEndpoints();
        return app;
    }

    private static void SeedProperties(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Properties.Add(new Property
        {
            Id = ExistingId,
            Title = "Casa con imagen",
            Description = "Descripción con imagen.",
            Address = "Avenida Central 456",
            Price = 320000m,
            Status = PropertyStatus.Available,
            BedroomCount = 4,
            BathroomCount = 3,
            AreaSquareMeters = 180m,
            ImageUrl = "/images/photo.png",
            CreatedAt = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2026, 2, 1, 10, 0, 0, DateTimeKind.Utc)
        });
        context.SaveChanges();
    }

    private static RouteEndpoint FindChangeStatusEndpoint(WebApplication app)
    {
        var dataSourcesProperty = typeof(WebApplication).GetProperty(
            "DataSources",
            BindingFlags.Instance | BindingFlags.NonPublic);
        var dataSources = Assert.IsAssignableFrom<ICollection<EndpointDataSource>>(
            dataSourcesProperty!.GetValue(app));

        return dataSources
            .SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>()
            .Single(candidate =>
                candidate.RoutePattern.RawText == "/api/properties/{id}/status" &&
                candidate.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods.Contains("PATCH") == true);
    }

    private static DefaultHttpContext CreateHttpContext(
        WebApplication app,
        Guid id,
        string json,
        string? rawId = null)
    {
        var routeId = rawId ?? id.ToString();
        var scope = app.Services.CreateScope();
        var context = new DefaultHttpContext
        {
            RequestServices = scope.ServiceProvider
        };
        scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext = context;
        context.Request.Method = "PATCH";
        context.Request.Path = $"/api/properties/{routeId}/status";
        context.Request.RouteValues["id"] = routeId;
        context.Request.ContentType = json.Length == 0 ? null : "application/json";
        context.Request.Scheme = "http";
        context.Request.Host = new HostString("localhost:5023");
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));
        context.Request.ContentLength = context.Request.Body.Length;
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<Property> ReadStoredPropertyAsync(WebApplication app, Guid id)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await context.Properties.AsNoTracking().SingleAsync(candidate => candidate.Id == id);
    }

    private static void AssertUnchanged(Property expected, Property actual)
    {
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Title, actual.Title);
        Assert.Equal(expected.Description, actual.Description);
        Assert.Equal(expected.Address, actual.Address);
        Assert.Equal(expected.Price, actual.Price);
        Assert.Equal(expected.Status, actual.Status);
        Assert.Equal(expected.BedroomCount, actual.BedroomCount);
        Assert.Equal(expected.BathroomCount, actual.BathroomCount);
        Assert.Equal(expected.AreaSquareMeters, actual.AreaSquareMeters);
        Assert.Equal(expected.ImageUrl, actual.ImageUrl);
        Assert.Equal(expected.CreatedAt, actual.CreatedAt);
        Assert.Equal(expected.UpdatedAt, actual.UpdatedAt);
    }

    private static async Task<JsonDocument> ReadJsonAsync(HttpContext context) =>
        JsonDocument.Parse(await ReadBodyAsync(context));

    private static async Task<string> ReadBodyAsync(HttpContext context)
    {
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        return await reader.ReadToEndAsync();
    }

    private sealed class StubPropertyImageStorage : IPropertyImageStorage
    {
        public Task<string> StoreAsync(IFormFile image, CancellationToken cancellationToken) =>
            Task.FromResult("/assets/properties/generated.png");

        public Task RemoveAsync(string imageUrl, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }
}
