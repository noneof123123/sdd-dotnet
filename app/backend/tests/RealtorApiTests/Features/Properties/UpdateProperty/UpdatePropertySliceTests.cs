using System.Reflection;
using System.Text;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RealtorApi.Features.Health;
using RealtorApi.Features.Properties.CreateProperty;
using RealtorApi.Features.Properties.ListProperties;
using RealtorApi.Features.Properties.UpdateProperty;
using RealtorApi.Infrastructure.Endpoints;
using RealtorApi.Infrastructure.Handlers;
using RealtorApi.Infrastructure.Persistence;
using RealtorApi.Infrastructure.Storage;

namespace RealtorApiTests.Features.Properties.UpdateProperty;

public sealed class UpdatePropertySliceTests
{
    private static readonly Guid ExistingId = Guid.Parse("3f8b1c2d-4e5a-4b6c-8d9e-0a1b2c3d4e5f");

    [Fact]
    public void SliceHandlerAndValidator_ArePublicConcreteAndDiscoverable()
    {
        Assert.True(typeof(ISlice).IsAssignableFrom(typeof(UpdatePropertySlice)));
        Assert.False(typeof(UpdatePropertySlice).IsAbstract);
        Assert.True(typeof(IHandler).IsAssignableFrom(typeof(UpdatePropertyHandler)));
        Assert.True(typeof(IValidator<UpdatePropertyRequest>).IsAssignableFrom(typeof(UpdatePropertyValidator)));
    }

    [Fact]
    public void Handler_IsRegisteredByAssemblyScanning()
    {
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.AddScoped<IPropertyImageStorage, StubPropertyImageStorage>();
        services.RegisterHandlers(typeof(UpdatePropertyHandler).Assembly);

        using var provider = services.BuildServiceProvider();
        var handlers = provider.GetServices<IHandler>();

        Assert.Contains(handlers, handler => handler is UpdatePropertyHandler);
    }

    [Fact]
    public void Slices_AreRegisteredByAssemblyScanningWithoutAffectingExistingSlices()
    {
        var services = new ServiceCollection();
        services.RegisterSlices(typeof(UpdatePropertySlice).Assembly);

        using var provider = services.BuildServiceProvider();
        var slices = provider.GetServices<ISlice>().ToArray();

        Assert.Contains(slices, slice => slice is UpdatePropertySlice);
        Assert.Contains(slices, slice => slice is CreatePropertySlice);
        Assert.Contains(slices, slice => slice is ListPropertiesSlice);
        Assert.Contains(slices, slice => slice is HealthSlice);
    }

    [Fact]
    public void AddEndpoint_MapsPutPropertyById()
    {
        var (app, _) = BuildApp();

        var endpoint = FindUpdateEndpoint(app);

        Assert.Equal("PUT", endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods.Single());
        Assert.Equal("/api/properties/{id}", endpoint.RoutePattern.RawText);
    }

    [Fact]
    public async Task PutWithoutImage_ReturnsOkWithUpdatedRepresentationAndPreservesImage()
    {
        var (app, storage) = BuildApp();
        var endpoint = FindUpdateEndpoint(app);
        var context = CreateHttpContext(app, ExistingId);

        context.Request.Body = CreateMultipartBody(includeImage: false);
        context.Request.ContentLength = context.Request.Body.Length;
        await endpoint.RequestDelegate!(context);

        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        Assert.Equal(0, storage.StoreCalls);
        Assert.Equal(0, storage.RemoveCalls);
        using var document = await ReadJsonAsync(context);
        Assert.Equal(ExistingId.ToString(), document.RootElement.GetProperty("id").GetString());
        Assert.Equal("Casa actualizada", document.RootElement.GetProperty("title").GetString());
        Assert.Equal("/assets/properties/old.png", document.RootElement.GetProperty("imageUrl").GetString());
        Assert.False(document.RootElement.TryGetProperty("createdAt", out _));
        Assert.False(document.RootElement.TryGetProperty("updatedAt", out _));
    }

    [Fact]
    public async Task PutWithPng_ReturnsOkWithRelativeImageUrlAndRemovesPrevious()
    {
        var (app, storage) = BuildApp();
        var endpoint = FindUpdateEndpoint(app);
        var context = CreateHttpContext(app, ExistingId);

        context.Request.Body = CreateMultipartBody(includeImage: true);
        context.Request.ContentLength = context.Request.Body.Length;
        await endpoint.RequestDelegate!(context);

        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        Assert.Equal(1, storage.StoreCalls);
        Assert.Equal(1, storage.RemoveCalls);
        var body = await ReadBodyAsync(context);
        using var document = JsonDocument.Parse(body);
        Assert.Equal("/assets/properties/generated.png", document.RootElement.GetProperty("imageUrl").GetString());
        Assert.DoesNotContain("photo.png", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("wwwroot", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(app.Environment.ContentRootPath, body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task PutWithJpeg_ReturnsOkWithRelativeImageUrl()
    {
        var (app, storage) = BuildApp();
        storage.StoredUrl = "/assets/properties/generated.jpg";
        var endpoint = FindUpdateEndpoint(app);
        var context = CreateHttpContext(app, ExistingId);
        byte[] jpegBytes = [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x01];

        context.Request.Body = CreateMultipartBody(
            includeImage: true,
            imageFileName: "photo.jpg",
            imageContentType: "image/jpeg",
            imageBytes: jpegBytes);
        context.Request.ContentLength = context.Request.Body.Length;
        await endpoint.RequestDelegate!(context);

        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        Assert.Equal(1, storage.StoreCalls);
        using var document = await ReadJsonAsync(context);
        Assert.Equal("/assets/properties/generated.jpg", document.RootElement.GetProperty("imageUrl").GetString());
    }

    [Fact]
    public async Task PutWithTwoImageParts_ReturnsBadRequestWithoutSideEffects()
    {
        var (app, storage) = BuildApp();
        var endpoint = FindUpdateEndpoint(app);
        var context = CreateHttpContext(app, ExistingId);

        context.Request.Body = CreateMultipartBody(includeImage: true, duplicateImage: true);
        context.Request.ContentLength = context.Request.Body.Length;
        await endpoint.RequestDelegate!(context);

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.Equal(0, storage.StoreCalls);
        using var document = await ReadJsonAsync(context);
        Assert.True(document.RootElement.TryGetProperty("errors", out var errors));
        Assert.True(errors.TryGetProperty("Image", out _));
        Assert.Equal("Casa original", await ReadStoredTitleAsync(app));
    }

    [Fact]
    public async Task PutWithMalformedId_ReturnsBadRequestWithoutSideEffects()
    {
        var (app, storage) = BuildApp();
        var endpoint = FindUpdateEndpoint(app);
        var context = CreateHttpContext(app, ExistingId, rawId: "no-es-un-guid");

        context.Request.Body = CreateMultipartBody(includeImage: false);
        context.Request.ContentLength = context.Request.Body.Length;
        await endpoint.RequestDelegate!(context);

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.Equal(0, storage.StoreCalls);
        Assert.Equal("Casa original", await ReadStoredTitleAsync(app));
    }

    [Fact]
    public async Task PutWithUnknownId_ReturnsNotFoundWithoutStorage()
    {
        var (app, storage) = BuildApp();
        var endpoint = FindUpdateEndpoint(app);
        var context = CreateHttpContext(app, Guid.NewGuid());

        context.Request.Body = CreateMultipartBody(includeImage: false);
        context.Request.ContentLength = context.Request.Body.Length;
        await endpoint.RequestDelegate!(context);

        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
        Assert.Equal(0, storage.StoreCalls);
        Assert.Equal(0, storage.RemoveCalls);
        Assert.Equal("Casa original", await ReadStoredTitleAsync(app));
    }

    [Fact]
    public async Task PutWithMalformedMultipartBody_ReturnsBadRequestWithoutSideEffects()
    {
        var (app, storage) = BuildApp();
        var endpoint = FindUpdateEndpoint(app);
        var context = CreateHttpContext(app, ExistingId);

        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("multipart-invalido"));
        context.Request.ContentLength = context.Request.Body.Length;
        await endpoint.RequestDelegate!(context);

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.Equal(0, storage.StoreCalls);
        Assert.Equal("Casa original", await ReadStoredTitleAsync(app));
    }

    [Fact]
    public async Task PutWhenStorageFails_ReturnsGenericInternalProblem()
    {
        var (app, storage) = BuildApp();
        storage.ThrowOnStore = true;
        var endpoint = FindUpdateEndpoint(app);
        var context = CreateHttpContext(app, ExistingId);

        context.Request.Body = CreateMultipartBody(includeImage: true);
        context.Request.ContentLength = context.Request.Body.Length;
        await endpoint.RequestDelegate!(context);

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        var body = await ReadBodyAsync(context);
        Assert.DoesNotContain("Simulación", body, StringComparison.Ordinal);
        Assert.DoesNotContain("IOException", body, StringComparison.Ordinal);
        Assert.Equal("Casa original", await ReadStoredTitleAsync(app));
    }

    private static (WebApplication App, StubPropertyImageStorage Storage) BuildApp()
    {
        var builder = WebApplication.CreateBuilder();
        var databaseName = Guid.NewGuid().ToString();
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(databaseName));
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddValidatorsFromAssembly(typeof(UpdatePropertyValidator).Assembly, ServiceLifetime.Scoped);
        builder.Services.RegisterSlices(typeof(UpdatePropertySlice).Assembly);
        builder.Services.RegisterHandlers(typeof(UpdatePropertyHandler).Assembly);
        var storage = new StubPropertyImageStorage();
        builder.Services.AddSingleton<IPropertyImageStorage>(storage);

        var app = builder.Build();
        SeedExistingProperty(app);
        app.MapSliceEndpoints();
        return (app, storage);
    }

    private static void SeedExistingProperty(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Properties.Add(new RealtorApi.Domain.Properties.Property
        {
            Id = ExistingId,
            Title = "Casa original",
            Description = "Descripción original.",
            Address = "Avenida Central 456",
            Price = 180000m,
            Status = RealtorApi.Domain.Properties.PropertyStatus.Available,
            BedroomCount = 2,
            BathroomCount = 1,
            AreaSquareMeters = 90m,
            ImageUrl = "/assets/properties/old.png",
            CreatedAt = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2026, 2, 1, 10, 0, 0, DateTimeKind.Utc)
        });
        context.SaveChanges();
    }

    private static RouteEndpoint FindUpdateEndpoint(WebApplication app)
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
                candidate.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods.Contains("PUT") == true);
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
        context.Request.Method = "PUT";
        context.Request.Path = $"/api/properties/{routeId}";
        context.Request.RouteValues["id"] = routeId;
        context.Request.ContentType = "multipart/form-data; boundary=realtor-update";
        context.Request.Scheme = "http";
        context.Request.Host = new HostString("localhost");
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static MemoryStream CreateMultipartBody(
        bool includeImage,
        bool duplicateImage = false,
        string imageFileName = "photo.png",
        string imageContentType = "image/png",
        byte[]? imageBytes = null)
    {
        const string boundary = "realtor-update";
        byte[] pngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
        imageBytes ??= pngSignature;
        var stream = new MemoryStream();
        using var writer = new StreamWriter(stream, new UTF8Encoding(false), leaveOpen: true);

        void AddField(string name, string value)
        {
            writer.Write($"--{boundary}\r\n");
            writer.Write($"Content-Disposition: form-data; name=\"{name}\"\r\n\r\n");
            writer.Write(value);
            writer.Write("\r\n");
        }

        void AddImage(string fileName)
        {
            writer.Write($"--{boundary}\r\n");
            writer.Write($"Content-Disposition: form-data; name=\"image\"; filename=\"{fileName}\"\r\n");
            writer.Write($"Content-Type: {imageContentType}\r\n\r\n");
            writer.Flush();
            stream.Write(imageBytes);
            writer.Write("\r\n");
        }

        AddField("title", "Casa actualizada");
        AddField("description", "Descripción vigente.");
        AddField("address", "Calle Central 123");
        AddField("price", "275000.00");
        AddField("status", "Maintenance");
        AddField("bedroomCount", "3");
        AddField("bathroomCount", "2");
        AddField("areaSquareMeters", "145.50");
        if (includeImage)
        {
            AddImage(imageFileName);
            if (duplicateImage)
                AddImage($"second{Path.GetExtension(imageFileName)}");
        }

        writer.Write($"--{boundary}--\r\n");
        writer.Flush();
        stream.Position = 0;
        return stream;
    }

    private static async Task<string> ReadStoredTitleAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await context.Properties
            .AsNoTracking()
            .Where(property => property.Id == ExistingId)
            .Select(property => property.Title)
            .SingleAsync();
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
        public int StoreCalls { get; private set; }

        public int RemoveCalls { get; private set; }

        public bool ThrowOnStore { get; set; }

        public string StoredUrl { get; set; } = "/assets/properties/generated.png";

        public Task<string> StoreAsync(IFormFile image, CancellationToken cancellationToken)
        {
            StoreCalls++;
            if (ThrowOnStore)
                throw new IOException("Simulación de fallo de almacenamiento.");
            return Task.FromResult(StoredUrl);
        }

        public Task RemoveAsync(string imageUrl, CancellationToken cancellationToken)
        {
            RemoveCalls++;
            return Task.CompletedTask;
        }
    }
}
