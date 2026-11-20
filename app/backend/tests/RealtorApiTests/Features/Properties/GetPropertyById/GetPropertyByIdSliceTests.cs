using System.Reflection;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using RealtorApi.Features.Health;
using RealtorApi.Features.Properties.CreateProperty;
using RealtorApi.Features.Properties.GetPropertyById;
using RealtorApi.Features.Properties.ListProperties;
using RealtorApi.Features.Properties.UpdateProperty;
using RealtorApi.Infrastructure.Endpoints;
using RealtorApi.Infrastructure.Handlers;
using RealtorApi.Infrastructure.Persistence;
using RealtorApi.Infrastructure.Storage;

namespace RealtorApiTests.Features.Properties.GetPropertyById;

public sealed class GetPropertyByIdSliceTests
{
    private static readonly Guid ExistingId = Guid.Parse("3f8b1c2d-4e5a-4b6c-8d9e-0a1b2c3d4e5f");
    private static readonly Guid NoImageId = Guid.Parse("2a7c4d1e-9b3f-4a82-8c17-6d5e4f3a2b19");

    [Fact]
    public void SliceHandlerAndValidator_ArePublicConcreteAndDiscoverable()
    {
        Assert.True(typeof(ISlice).IsAssignableFrom(typeof(GetPropertyByIdSlice)));
        Assert.False(typeof(GetPropertyByIdSlice).IsAbstract);
        Assert.True(typeof(IHandler).IsAssignableFrom(typeof(GetPropertyByIdHandler)));
        Assert.True(typeof(IValidator<GetPropertyByIdRequest>).IsAssignableFrom(typeof(GetPropertyByIdValidator)));
    }

    [Fact]
    public void Handler_IsRegisteredByAssemblyScanning()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.AddSingleton<IPropertyImageStorage, StubPropertyImageStorage>();
        services.RegisterHandlers(typeof(GetPropertyByIdHandler).Assembly);

        using var provider = services.BuildServiceProvider();

        Assert.Contains(provider.GetServices<IHandler>(), handler => handler is GetPropertyByIdHandler);
    }

    [Fact]
    public void Slices_AreRegisteredByAssemblyScanningWithoutAffectingExistingSlices()
    {
        var services = new ServiceCollection();
        services.RegisterSlices(typeof(GetPropertyByIdSlice).Assembly);

        using var provider = services.BuildServiceProvider();
        var slices = provider.GetServices<ISlice>().ToArray();

        Assert.Contains(slices, slice => slice is GetPropertyByIdSlice);
        Assert.Contains(slices, slice => slice is CreatePropertySlice);
        Assert.Contains(slices, slice => slice is ListPropertiesSlice);
        Assert.Contains(slices, slice => slice is UpdatePropertySlice);
        Assert.Contains(slices, slice => slice is HealthSlice);
    }

    [Fact]
    public void AddEndpoint_MapsGetPropertyById()
    {
        var app = BuildApp();

        var endpoint = FindGetEndpoint(app);

        Assert.Equal("GET", endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods.Single());
        Assert.Equal("/api/properties/{id}", endpoint.RoutePattern.RawText);
    }

    [Fact]
    public async Task GetWithExistingProperty_ReturnsOkWithAllDetailFieldsAndAbsoluteImageUrl()
    {
        var app = BuildApp();
        var endpoint = FindGetEndpoint(app);
        var context = CreateHttpContext(app, ExistingId);

        await endpoint.RequestDelegate!(context);

        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        using var document = await ReadJsonAsync(context);
        var root = document.RootElement;
        Assert.Equal(ExistingId.ToString(), root.GetProperty("id").GetString());
        Assert.Equal("Casa con imagen", root.GetProperty("title").GetString());
        Assert.Equal("Descripción con imagen.", root.GetProperty("description").GetString());
        Assert.Equal("Avenida Central 456", root.GetProperty("address").GetString());
        Assert.Equal(320000m, root.GetProperty("price").GetDecimal());
        Assert.Equal("Available", root.GetProperty("status").GetString());
        Assert.Equal(4, root.GetProperty("bedroomCount").GetInt32());
        Assert.Equal(3, root.GetProperty("bathroomCount").GetInt32());
        Assert.Equal(180m, root.GetProperty("areaSquareMeters").GetDecimal());
        Assert.Equal(
            "http://localhost:5023/assets/properties/photo.png",
            root.GetProperty("imageUrl").GetString());
        Assert.Equal("2026-01-01T10:00:00Z", root.GetProperty("createdAt").GetString());
        Assert.Equal("2026-02-01T10:00:00Z", root.GetProperty("updatedAt").GetString());
        Assert.False(root.TryGetProperty("items", out _));
        Assert.False(root.TryGetProperty("totalItems", out _));
    }

    [Fact]
    public async Task GetWithoutImage_ReturnsOkWithEmptyImageUrlAndNullUpdatedAt()
    {
        var app = BuildApp();
        var endpoint = FindGetEndpoint(app);
        var context = CreateHttpContext(app, NoImageId);

        await endpoint.RequestDelegate!(context);

        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        using var document = await ReadJsonAsync(context);
        Assert.Equal(string.Empty, document.RootElement.GetProperty("imageUrl").GetString());
        Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("updatedAt").ValueKind);
    }

    [Fact]
    public async Task GetWithUnknownId_ReturnsNotFoundWithoutChanges()
    {
        var app = BuildApp();
        var endpoint = FindGetEndpoint(app);
        var before = await ReadStoredPropertyAsync(app, ExistingId);
        var context = CreateHttpContext(app, Guid.NewGuid());

        await endpoint.RequestDelegate!(context);

        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
        using var document = await ReadJsonAsync(context);
        Assert.Equal(404, document.RootElement.GetProperty("status").GetInt32());
        AssertUnchanged(before, await ReadStoredPropertyAsync(app, ExistingId));
    }

    [Fact]
    public async Task GetWithEmptyId_ReturnsBadRequestWithValidationErrors()
    {
        var app = BuildApp();
        var endpoint = FindGetEndpoint(app);
        var context = CreateHttpContext(app, Guid.Empty);

        await endpoint.RequestDelegate!(context);

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        using var document = await ReadJsonAsync(context);
        Assert.True(document.RootElement.TryGetProperty("errors", out var errors));
        Assert.True(errors.TryGetProperty("Id", out _));
    }

    [Fact]
    public async Task GetWithMalformedId_ReturnsBadRequest()
    {
        var app = BuildApp();
        var endpoint = FindGetEndpoint(app);
        var context = CreateHttpContext(app, Guid.Empty, rawId: "no-es-un-guid");

        await endpoint.RequestDelegate!(context);

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
    }

    [Fact]
    public async Task GetWithExistingProperty_DoesNotExposePhysicalPaths()
    {
        var app = BuildApp();
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var property = await dbContext.Properties.SingleAsync(candidate => candidate.Id == ExistingId);
        property.ImageUrl = "/var/www/private/photo.png";
        await dbContext.SaveChangesAsync();
        var endpoint = FindGetEndpoint(app);
        var context = CreateHttpContext(app, ExistingId);

        await endpoint.RequestDelegate!(context);

        var body = await ReadBodyAsync(context);
        Assert.DoesNotContain("/var/www", body, StringComparison.Ordinal);
        Assert.DoesNotContain("wwwroot", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(app.Environment.ContentRootPath, body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetWhenHandlerFails_ReturnsGenericInternalProblem()
    {
        var app = BuildApp(useFailingHandler: true);
        var endpoint = FindGetEndpoint(app);
        var context = CreateHttpContext(app, ExistingId);

        await endpoint.RequestDelegate!(context);

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        var body = await ReadBodyAsync(context);
        Assert.DoesNotContain("ObjectDisposed", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", body, StringComparison.OrdinalIgnoreCase);
    }

    private static WebApplication BuildApp(bool useFailingHandler = false)
    {
        var builder = WebApplication.CreateBuilder();
        var databaseName = Guid.NewGuid().ToString();
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(databaseName));
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddValidatorsFromAssembly(typeof(GetPropertyByIdValidator).Assembly, ServiceLifetime.Scoped);

        if (useFailingHandler)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var failingContext = new AppDbContext(options);
            failingContext.Dispose();
            builder.Services.AddSingleton(new GetPropertyByIdHandler(failingContext, NullLogger<GetPropertyByIdHandler>.Instance));
        }

        builder.Services.RegisterSlices(typeof(GetPropertyByIdSlice).Assembly);
        builder.Services.RegisterHandlers(typeof(GetPropertyByIdHandler).Assembly);

        var app = builder.Build();
        if (!useFailingHandler)
            SeedProperties(app);
        app.MapSliceEndpoints();
        return app;
    }

    private static void SeedProperties(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Properties.AddRange(
            new RealtorApi.Domain.Properties.Property
            {
                Id = ExistingId,
                Title = "Casa con imagen",
                Description = "Descripción con imagen.",
                Address = "Avenida Central 456",
                Price = 320000m,
                Status = RealtorApi.Domain.Properties.PropertyStatus.Available,
                BedroomCount = 4,
                BathroomCount = 3,
                AreaSquareMeters = 180m,
                ImageUrl = "/images/photo.png",
                CreatedAt = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2026, 2, 1, 10, 0, 0, DateTimeKind.Utc)
            },
            new RealtorApi.Domain.Properties.Property
            {
                Id = NoImageId,
                Title = "Casa sin imagen",
                Description = "Descripción sin imagen.",
                Address = "Calle Central 123",
                Price = 250000m,
                Status = RealtorApi.Domain.Properties.PropertyStatus.Rented,
                BedroomCount = 3,
                BathroomCount = 2,
                AreaSquareMeters = 140.50m,
                ImageUrl = null,
                CreatedAt = new DateTime(2026, 1, 1, 9, 30, 0, DateTimeKind.Utc),
                UpdatedAt = null
            });
        context.SaveChanges();
    }

    private static RouteEndpoint FindGetEndpoint(WebApplication app)
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
                candidate.RoutePattern.RawText == "/api/properties/{id}" &&
                candidate.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods.Contains("GET") == true);
    }

    private static DefaultHttpContext CreateHttpContext(WebApplication app, Guid id, string? rawId = null)
    {
        var routeId = rawId ?? id.ToString();
        var scope = app.Services.CreateScope();
        var context = new DefaultHttpContext
        {
            RequestServices = scope.ServiceProvider
        };
        scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext = context;
        context.Request.Method = "GET";
        context.Request.Path = $"/api/properties/{routeId}";
        context.Request.RouteValues["id"] = routeId;
        context.Request.Scheme = "http";
        context.Request.Host = new HostString("localhost:5023");
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<RealtorApi.Domain.Properties.Property> ReadStoredPropertyAsync(
        WebApplication app,
        Guid id)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await context.Properties.AsNoTracking().SingleAsync(property => property.Id == id);
    }

    private static void AssertUnchanged(
        RealtorApi.Domain.Properties.Property expected,
        RealtorApi.Domain.Properties.Property actual)
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
