using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RealtorApi.Domain.Properties;
using RealtorApi.Features.Properties.CreateProperty;
using RealtorApi.Features.Properties.ListProperties;
using RealtorApi.Infrastructure.Persistence;
using RealtorApi.Infrastructure.Results;
using RealtorApi.Infrastructure.Storage;

namespace RealtorApiTests.Features.Properties.CreateProperty;

public sealed class CreatePropertyHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithoutImage_PersistsPropertyAndDoesNotUseStorage()
    {
        await using var context = CreateContext();
        var storage = new StubPropertyImageStorage();
        var handler = new CreatePropertyHandler(context, storage);
        var before = DateTime.UtcNow;

        var result = await handler.HandleAsync(CreateRequest(), CancellationToken.None);

        var after = DateTime.UtcNow;
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Null(result.Value!.ImageUrl);
        Assert.Equal(0, storage.StoreCalls);
        Assert.Equal(0, storage.RemoveCalls);

        var persisted = Assert.Single(await context.Properties.AsNoTracking().ToListAsync());
        Assert.Equal(result.Value.Id, persisted.Id);
        Assert.Null(persisted.ImageUrl);
        Assert.InRange(persisted.CreatedAt, before, after);
        Assert.Null(persisted.UpdatedAt);
    }

    [Fact]
    public async Task HandleAsync_WithImage_PersistsReturnedRelativeUrl()
    {
        await using var context = CreateContext();
        var storage = new StubPropertyImageStorage
        {
            StoredUrl = "/assets/properties/generated.png"
        };
        var handler = new CreatePropertyHandler(context, storage);
        var request = CreateRequest() with { Image = CreateImage("property.png", "image/png") };

        var result = await handler.HandleAsync(request, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("/assets/properties/generated.png", result.Value!.ImageUrl);
        Assert.Equal(1, storage.StoreCalls);
        Assert.Equal(0, storage.RemoveCalls);
        Assert.Equal("/assets/properties/generated.png", (await context.Properties.SingleAsync()).ImageUrl);
    }

    [Fact]
    public async Task HandleAsync_WhenPersistenceFails_RemovesStoredImageAndReturnsInternalError()
    {
        var context = CreateContext();
        context.Dispose();
        var storage = new StubPropertyImageStorage
        {
            StoredUrl = "/assets/properties/generated.png"
        };
        var handler = new CreatePropertyHandler(context, storage);
        var request = CreateRequest() with { Image = CreateImage("property.png", "image/png") };

        var result = await handler.HandleAsync(request, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Internal, result.Error!.Type);
        Assert.Equal(1, storage.StoreCalls);
        Assert.Equal(1, storage.RemoveCalls);
    }

    [Fact]
    public async Task HandleAsync_WhenStorageFails_ReturnsInternalErrorAndDoesNotPersist()
    {
        await using var context = CreateContext();
        var storage = new StubPropertyImageStorage
        {
            StoreException = new IOException("Simulación de fallo de almacenamiento.")
        };
        var handler = new CreatePropertyHandler(context, storage);
        var request = CreateRequest() with { Image = CreateImage("property.png", "image/png") };

        var result = await handler.HandleAsync(request, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Internal, result.Error!.Type);
        Assert.Equal(1, storage.StoreCalls);
        Assert.Equal(0, storage.RemoveCalls);
        Assert.Empty(await context.Properties.ToListAsync());
    }

    [Fact]
    public async Task HandleAsync_WhenCancelledAfterStoring_PropagatesAndRemovesStoredImage()
    {
        await using var context = CreateContext();
        var storage = new StubPropertyImageStorage
        {
            StoredUrl = "/assets/properties/generated.png",
            IgnoreCancellation = true
        };
        var handler = new CreatePropertyHandler(context, storage);
        var request = CreateRequest() with { Image = CreateImage("property.png", "image/png") };
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => handler.HandleAsync(request, cancellation.Token));

        Assert.Equal(1, storage.StoreCalls);
        Assert.Equal(1, storage.RemoveCalls);
        Assert.Empty(await context.Properties.ToListAsync());
    }

    [Fact]
    public async Task HandleAsync_CreatedProperty_IsVisibleThroughListHandler()
    {
        await using var context = CreateContext();
        var storage = new StubPropertyImageStorage
        {
            StoredUrl = "/assets/properties/generated.png"
        };
        var createHandler = new CreatePropertyHandler(context, storage);
        var request = CreateRequest() with { Image = CreateImage("property.png", "image/png") };

        var created = await createHandler.HandleAsync(request, CancellationToken.None);
        var listHandler = new ListPropertiesHandler(context);
        var httpRequest = new DefaultHttpContext().Request;
        httpRequest.Scheme = "http";
        httpRequest.Host = new HostString("example.com");

        var listed = await listHandler.HandleAsync(new ListPropertiesRequest(), httpRequest, CancellationToken.None);

        Assert.True(created.IsSuccess);
        var item = Assert.Single(listed.Items);
        Assert.Equal(created.Value!.Id, item.Id);
        Assert.Equal("http://example.com/assets/properties/generated.png", item.ImageUrl);
    }

    private static CreatePropertyRequest CreateRequest() => new()
    {
        Title = "Casa de prueba",
        Description = "Propiedad creada para validación.",
        Address = "Calle Central 123",
        Price = 250000.00m,
        Status = PropertyStatus.Available,
        BedroomCount = 3,
        BathroomCount = 2,
        AreaSquareMeters = 140.50m
    };

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static IFormFile CreateImage(string fileName, string contentType)
    {
        byte[] bytes = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x01, 0x02];
        return new FormFile(new MemoryStream(bytes), 0, bytes.Length, "image", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }

    private sealed class StubPropertyImageStorage : IPropertyImageStorage
    {
        public int StoreCalls { get; private set; }

        public int RemoveCalls { get; private set; }

        public string StoredUrl { get; set; } = "/assets/properties/stub.png";

        public Exception? StoreException { get; set; }

        public bool IgnoreCancellation { get; set; }

        public Task<string> StoreAsync(IFormFile image, CancellationToken cancellationToken)
        {
            StoreCalls++;
            if (!IgnoreCancellation)
                cancellationToken.ThrowIfCancellationRequested();
            if (StoreException is not null)
                throw StoreException;
            return Task.FromResult(StoredUrl);
        }

        public Task RemoveAsync(string imageUrl, CancellationToken cancellationToken)
        {
            RemoveCalls++;
            return Task.CompletedTask;
        }
    }
}
