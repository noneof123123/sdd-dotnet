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
using RealtorApi.Infrastructure.Endpoints;
using RealtorApi.Infrastructure.Handlers;
using RealtorApi.Infrastructure.Persistence;
using RealtorApi.Infrastructure.Storage;

namespace RealtorApiTests.Features.Properties.CreateProperty;

public sealed class CreatePropertySliceTests
{
    [Fact]
    public void Slice_IsPublicConcreteAndDiscoverable()
    {
        Assert.True(typeof(ISlice).IsAssignableFrom(typeof(CreatePropertySlice)));
        Assert.False(typeof(CreatePropertySlice).IsAbstract);
        Assert.True(typeof(IHandler).IsAssignableFrom(typeof(CreatePropertyHandler)));
    }

    [Fact]
    public void Handler_IsRegisteredByAssemblyScanning()
    {
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.AddScoped<IPropertyImageStorage, StubPropertyImageStorage>();
        services.RegisterHandlers(typeof(CreatePropertyHandler).Assembly);

        using var provider = services.BuildServiceProvider();
        var handlers = provider.GetServices<IHandler>();

        Assert.Contains(handlers, handler => handler is CreatePropertyHandler);
    }

    [Fact]
    public void Slices_AreRegisteredByAssemblyScanningWithoutAffectingExistingSlices()
    {
        var services = new ServiceCollection();
        services.RegisterSlices(typeof(CreatePropertySlice).Assembly);

        using var provider = services.BuildServiceProvider();
        var slices = provider.GetServices<ISlice>().ToArray();

        Assert.Contains(slices, slice => slice is CreatePropertySlice);
        Assert.Contains(slices, slice => slice is HealthSlice);
        Assert.Contains(slices, slice => slice is ListPropertiesSlice);
    }

    [Fact]
    public void AddEndpoint_MapsPostProperties()
    {
        var (app, _) = BuildApp();

        var endpoint = FindCreateEndpoint(app);

        Assert.Equal("POST", endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods.Single());
    }

    [Fact]
    public async Task PostWithoutImage_ReturnsCreatedWithNullImageUrl()
    {
        var (app, storage) = BuildApp();
        var endpoint = FindCreateEndpoint(app);
        var context = CreateHttpContext(app);

        context.Request.Body = CreateMultipartBody(includeImage: false);
        context.Request.ContentLength = context.Request.Body.Length;
        await endpoint.RequestDelegate!(context);

        Assert.Equal(StatusCodes.Status201Created, context.Response.StatusCode);
        Assert.StartsWith("/api/properties/", context.Response.Headers.Location.ToString(), StringComparison.Ordinal);
        Assert.Equal(0, storage.StoreCalls);

        using var document = await ReadJsonAsync(context);
        Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("imageUrl").ValueKind);
        Assert.True(document.RootElement.TryGetProperty("id", out _));
    }

    [Fact]
    public async Task PostWithPng_ReturnsCreatedWithRelativeImageUrl()
    {
        var (app, storage) = BuildApp();
        var endpoint = FindCreateEndpoint(app);
        var context = CreateHttpContext(app);

        context.Request.Body = CreateMultipartBody(includeImage: true);
        context.Request.ContentLength = context.Request.Body.Length;
        await endpoint.RequestDelegate!(context);

        Assert.Equal(StatusCodes.Status201Created, context.Response.StatusCode);
        Assert.Equal(1, storage.StoreCalls);
        using var document = await ReadJsonAsync(context);
        Assert.Equal("/assets/properties/generated.png", document.RootElement.GetProperty("imageUrl").GetString());
    }

    [Fact]
    public async Task PostWithJpeg_ReturnsCreatedWithRelativeImageUrl()
    {
        var (app, storage) = BuildApp();
        storage.StoredUrl = "/assets/properties/generated.jpg";
        var endpoint = FindCreateEndpoint(app);
        var context = CreateHttpContext(app);

        byte[] jpegSignature = [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x01];
        context.Request.Body = CreateMultipartBody(
            includeImage: true,
            imageFileName: "photo.jpg",
            imageContentType: "image/jpeg",
            imageBytes: jpegSignature);
        context.Request.ContentLength = context.Request.Body.Length;
        await endpoint.RequestDelegate!(context);

        Assert.Equal(StatusCodes.Status201Created, context.Response.StatusCode);
        Assert.Equal(1, storage.StoreCalls);
        var body = await ReadBodyAsync(context);
        using var document = JsonDocument.Parse(body);
        Assert.Equal("/assets/properties/generated.jpg", document.RootElement.GetProperty("imageUrl").GetString());
        Assert.DoesNotContain("wwwroot", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(app.Environment.ContentRootPath, body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task PostWithTwoImageParts_ReturnsBadRequest()
    {
        var (app, storage) = BuildApp();
        var endpoint = FindCreateEndpoint(app);
        var context = CreateHttpContext(app);

        context.Request.Body = CreateMultipartBody(includeImage: true, duplicateImage: true);
        context.Request.ContentLength = context.Request.Body.Length;
        await endpoint.RequestDelegate!(context);

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.Equal(0, storage.StoreCalls);
        Assert.Equal(0, await CountPropertiesAsync(app));
        using var document = await ReadJsonAsync(context);
        Assert.True(document.RootElement.TryGetProperty("errors", out var errors));
        Assert.True(errors.TryGetProperty("Image", out _));
    }

    [Fact]
    public async Task PostWithMalformedMultipartBody_ReturnsBadRequestWithoutSideEffects()
    {
        var (app, storage) = BuildApp();
        var endpoint = FindCreateEndpoint(app);
        var context = CreateHttpContext(app);

        context.Request.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("multipart-invalido"));
        context.Request.ContentLength = context.Request.Body.Length;
        await endpoint.RequestDelegate!(context);

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.Equal(0, storage.StoreCalls);
        Assert.Equal(0, await CountPropertiesAsync(app));
    }

    [Fact]
    public async Task PostWhenStorageFails_ReturnsGenericInternalProblem()
    {
        var (app, storage) = BuildApp();
        storage.ThrowOnStore = true;
        var endpoint = FindCreateEndpoint(app);
        var context = CreateHttpContext(app);

        context.Request.Body = CreateMultipartBody(includeImage: true);
        context.Request.ContentLength = context.Request.Body.Length;
        await endpoint.RequestDelegate!(context);

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        var body = await ReadBodyAsync(context);
        Assert.DoesNotContain("Simulación", body, StringComparison.Ordinal);
        Assert.DoesNotContain("IOException", body, StringComparison.Ordinal);
    }

    private static (WebApplication App, StubPropertyImageStorage Storage) BuildApp()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddValidatorsFromAssembly(typeof(CreatePropertyValidator).Assembly, ServiceLifetime.Scoped);
        builder.Services.RegisterSlices(typeof(CreatePropertySlice).Assembly);
        builder.Services.RegisterHandlers(typeof(CreatePropertyHandler).Assembly);
        var storage = new StubPropertyImageStorage();
        builder.Services.AddSingleton<IPropertyImageStorage>(storage);

        var app = builder.Build();
        app.MapSliceEndpoints();
        return (app, storage);
    }

    private static RouteEndpoint FindCreateEndpoint(WebApplication app)
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
                candidate.RoutePattern.RawText == "/api/properties" &&
                candidate.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods.Contains("POST") == true);
    }

    private static DefaultHttpContext CreateHttpContext(WebApplication app)
    {
        var scope = app.Services.CreateScope();
        var context = new DefaultHttpContext
        {
            RequestServices = scope.ServiceProvider
        };
        scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext = context;
        context.Request.Method = "POST";
        context.Request.ContentType = "multipart/form-data; boundary=realtor-create";
        context.Request.Scheme = "http";
        context.Request.Host = new HostString("localhost");
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<int> CountPropertiesAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await context.Properties.CountAsync();
    }

    private static MemoryStream CreateMultipartBody(
        bool includeImage,
        bool duplicateImage = false,
        string imageFileName = "photo.png",
        string imageContentType = "image/png",
        byte[]? imageBytes = null)
    {
        const string boundary = "realtor-create";
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

        AddField("title", "Casa de prueba");
        AddField("description", "Propiedad creada para validación.");
        AddField("address", "Calle Central 123");
        AddField("price", "250000.00");
        AddField("status", "Available");
        AddField("bedroomCount", "3");
        AddField("bathroomCount", "2");
        AddField("areaSquareMeters", "140.50");
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

    private static async Task<JsonDocument> ReadJsonAsync(HttpContext context)
    {
        var body = await ReadBodyAsync(context);
        return JsonDocument.Parse(body);
    }

    private static async Task<string> ReadBodyAsync(HttpContext context)
    {
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        return await reader.ReadToEndAsync();
    }

    private sealed class StubPropertyImageStorage : IPropertyImageStorage
    {
        public int StoreCalls { get; private set; }

        public bool ThrowOnStore { get; set; }

        public string StoredUrl { get; set; } = "/assets/properties/generated.png";

        public Task<string> StoreAsync(IFormFile image, CancellationToken cancellationToken)
        {
            StoreCalls++;
            if (ThrowOnStore)
                throw new IOException("Simulación de fallo de almacenamiento.");
            return Task.FromResult(StoredUrl);
        }

        public Task RemoveAsync(string imageUrl, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }
}
