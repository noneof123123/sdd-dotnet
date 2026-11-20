using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RealtorApi.Domain.Properties;
using RealtorApi.Features.Properties.ListProperties;
using RealtorApi.Features.Properties.UpdateProperty;
using RealtorApi.Infrastructure.Persistence;
using RealtorApi.Infrastructure.Results;
using RealtorApi.Infrastructure.Storage;

namespace RealtorApiTests.Features.Properties.UpdateProperty;

public sealed class UpdatePropertyHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithoutImage_UpdatesFieldsPreservesInvariantsAndSkipsStorage()
    {
        await using var context = CreateContext();
        var existing = CreateExisting(Guid.NewGuid(), "/assets/properties/old.png");
        await SeedAsync(context, existing);
        var storage = new StubPropertyImageStorage();
        var handler = new UpdatePropertyHandler(context, storage);
        var before = DateTime.UtcNow;

        var result = await handler.HandleAsync(CreateRequest(existing.Id), CancellationToken.None);

        var after = DateTime.UtcNow;
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(existing.Id, result.Value!.Id);
        Assert.Equal("/assets/properties/old.png", result.Value.ImageUrl);
        Assert.Equal(0, storage.StoreCalls);
        Assert.Equal(0, storage.RemoveCalls);

        var persisted = await context.Properties.AsNoTracking().SingleAsync();
        Assert.Equal(existing.Id, persisted.Id);
        Assert.Equal(existing.CreatedAt, persisted.CreatedAt);
        Assert.Equal("/assets/properties/old.png", persisted.ImageUrl);
        Assert.Equal("Casa actualizada", persisted.Title);
        Assert.Equal(PropertyStatus.Maintenance, persisted.Status);
        Assert.InRange(persisted.UpdatedAt!.Value, before, after);
        Assert.Equal(DateTimeKind.Utc, persisted.UpdatedAt.Value.Kind);
    }

    [Fact]
    public async Task HandleAsync_TwoSequentialUpdates_AppliesLastWriteWins()
    {
        await using var context = CreateContext();
        var existing = CreateExisting(Guid.NewGuid());
        await SeedAsync(context, existing);
        var storage = new StubPropertyImageStorage();
        var handler = new UpdatePropertyHandler(context, storage);

        var first = await handler.HandleAsync(CreateRequest(existing.Id), CancellationToken.None);
        var second = await handler.HandleAsync(
            CreateRequest(existing.Id) with { Title = "Segunda escritura", Price = 310000m },
            CancellationToken.None);

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        var persisted = await context.Properties.AsNoTracking().SingleAsync();
        Assert.Equal(existing.Id, persisted.Id);
        Assert.Equal("Segunda escritura", persisted.Title);
        Assert.Equal(310000m, persisted.Price);
    }

    [Theory]
    [InlineData("photo.png", "image/png")]
    [InlineData("photo.jpg", "image/jpeg")]
    [InlineData("photo.jpeg", "image/jpeg")]
    public async Task HandleAsync_WithValidImage_PublishesPersistsAndRemovesPrevious(
        string fileName,
        string contentType)
    {
        await using var context = CreateContext();
        var existing = CreateExisting(Guid.NewGuid(), "/assets/properties/old.png");
        await SeedAsync(context, existing);
        var storage = new StubPropertyImageStorage
        {
            StoredUrl = "/assets/properties/generated.png"
        };
        var handler = new UpdatePropertyHandler(context, storage);

        var result = await handler.HandleAsync(
            CreateRequest(existing.Id) with { Image = CreateImage(fileName, contentType) },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("/assets/properties/generated.png", result.Value!.ImageUrl);
        Assert.Equal(1, storage.StoreCalls);
        Assert.Equal(1, storage.RemoveCalls);
        Assert.Equal("/assets/properties/old.png", Assert.Single(storage.RemovedUrls));
        var persisted = await context.Properties.AsNoTracking().SingleAsync();
        Assert.Equal("/assets/properties/generated.png", persisted.ImageUrl);
        Assert.Equal(existing.CreatedAt, persisted.CreatedAt);
    }

    [Fact]
    public async Task HandleAsync_WhenStorageFailsBeforePublishing_ReturnsInternalWithoutChanges()
    {
        await using var context = CreateContext();
        var existing = CreateExisting(Guid.NewGuid(), "/assets/properties/old.png");
        await SeedAsync(context, existing);
        var storage = new StubPropertyImageStorage
        {
            StoreException = new IOException("Simulación de fallo de almacenamiento.")
        };
        var handler = new UpdatePropertyHandler(context, storage);

        var result = await handler.HandleAsync(
            CreateRequest(existing.Id) with { Image = CreateImage("photo.png", "image/png") },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Internal, result.Error!.Type);
        Assert.Equal(1, storage.StoreCalls);
        Assert.Equal(0, storage.RemoveCalls);
        var persisted = await context.Properties.AsNoTracking().SingleAsync();
        Assert.Equal("Casa original", persisted.Title);
        Assert.Equal("/assets/properties/old.png", persisted.ImageUrl);
        Assert.Equal(existing.UpdatedAt, persisted.UpdatedAt);
    }

    [Fact]
    public async Task HandleAsync_WhenPersistenceFailsAfterPublishing_RemovesNewImageAndReturnsInternal()
    {
        await using var context = CreateContext();
        var existing = CreateExisting(Guid.NewGuid(), "/assets/properties/old.png");
        await SeedAsync(context, existing);
        var storage = new StubPropertyImageStorage
        {
            StoredUrl = "/assets/properties/generated.png",
            OnStore = () => context.Dispose()
        };
        var handler = new UpdatePropertyHandler(context, storage);

        var result = await handler.HandleAsync(
            CreateRequest(existing.Id) with { Image = CreateImage("photo.png", "image/png") },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Internal, result.Error!.Type);
        Assert.Equal(1, storage.StoreCalls);
        Assert.Equal("/assets/properties/generated.png", Assert.Single(storage.RemovedUrls));
    }

    [Fact]
    public async Task HandleAsync_WhenPreviousImageRemovalFails_RestoresSnapshotAndReturnsInternal()
    {
        await using var context = CreateContext();
        var existing = CreateExisting(Guid.NewGuid(), "/assets/properties/old.png");
        await SeedAsync(context, existing);
        var storage = new StubPropertyImageStorage
        {
            StoredUrl = "/assets/properties/generated.png",
            RemoveException = new IOException("Simulación de fallo al retirar.")
        };
        var handler = new UpdatePropertyHandler(context, storage);

        var result = await handler.HandleAsync(
            CreateRequest(existing.Id) with { Image = CreateImage("photo.png", "image/png") },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Internal, result.Error!.Type);
        Assert.Equal(1, storage.StoreCalls);
        Assert.Equal(2, storage.RemoveCalls);
        var persisted = await context.Properties.AsNoTracking().SingleAsync();
        Assert.Equal(existing.Title, persisted.Title);
        Assert.Equal(existing.Description, persisted.Description);
        Assert.Equal(existing.Price, persisted.Price);
        Assert.Equal(existing.ImageUrl, persisted.ImageUrl);
        Assert.Equal(existing.UpdatedAt, persisted.UpdatedAt);
    }

    [Fact]
    public async Task HandleAsync_WhenCancelledBeforeHandling_PropagatesWithoutStorageCalls()
    {
        await using var context = CreateContext();
        var existing = CreateExisting(Guid.NewGuid(), "/assets/properties/old.png");
        await SeedAsync(context, existing);
        var storage = new StubPropertyImageStorage();
        var handler = new UpdatePropertyHandler(context, storage);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            handler.HandleAsync(
                CreateRequest(existing.Id) with { Image = CreateImage("photo.png", "image/png") },
                cancellation.Token));

        Assert.Equal(0, storage.StoreCalls);
        Assert.Equal(0, storage.RemoveCalls);
        var persisted = await context.Properties.AsNoTracking().SingleAsync();
        Assert.Equal(existing.Title, persisted.Title);
    }

    [Fact]
    public async Task HandleAsync_WhenCancelledAfterPublishing_PropagatesAndRemovesNewImage()
    {
        await using var context = CreateContext();
        var existing = CreateExisting(Guid.NewGuid(), "/assets/properties/old.png");
        await SeedAsync(context, existing);
        using var cancellation = new CancellationTokenSource();
        var storage = new StubPropertyImageStorage
        {
            StoredUrl = "/assets/properties/generated.png",
            IgnoreCancellation = true,
            OnStore = () => cancellation.Cancel()
        };
        var handler = new UpdatePropertyHandler(context, storage);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            handler.HandleAsync(
                CreateRequest(existing.Id) with { Image = CreateImage("photo.png", "image/png") },
                cancellation.Token));

        Assert.Equal(1, storage.StoreCalls);
        Assert.Equal(1, storage.RemoveCalls);
        Assert.Equal("/assets/properties/generated.png", Assert.Single(storage.RemovedUrls));
        var persisted = await context.Properties.AsNoTracking().SingleAsync();
        Assert.Equal(existing.Title, persisted.Title);
        Assert.Equal(existing.ImageUrl, persisted.ImageUrl);
    }

    [Fact]
    public async Task HandleAsync_UpdatedProperty_IsVisibleThroughListHandler()
    {
        await using var context = CreateContext();
        var existing = CreateExisting(Guid.NewGuid());
        await SeedAsync(context, existing);
        var storage = new StubPropertyImageStorage
        {
            StoredUrl = "/assets/properties/generated.png"
        };
        var handler = new UpdatePropertyHandler(context, storage);

        var updated = await handler.HandleAsync(
            CreateRequest(existing.Id) with { Image = CreateImage("photo.png", "image/png") },
            CancellationToken.None);
        var listHandler = new ListPropertiesHandler(context);
        var httpRequest = new DefaultHttpContext().Request;
        httpRequest.Scheme = "http";
        httpRequest.Host = new HostString("example.com");

        var listed = await listHandler.HandleAsync(new ListPropertiesRequest(), httpRequest, CancellationToken.None);

        Assert.True(updated.IsSuccess);
        var item = Assert.Single(listed.Items);
        Assert.Equal(existing.Id, item.Id);
        Assert.Equal("Casa actualizada", item.Title);
        Assert.Equal("http://example.com/assets/properties/generated.png", item.ImageUrl);
    }

    private static UpdatePropertyRequest CreateRequest(Guid id) => new()
    {
        Id = id,
        Title = "Casa actualizada",
        Description = "Descripción vigente.",
        Address = "Calle Central 123",
        Price = 275000.00m,
        Status = PropertyStatus.Maintenance,
        BedroomCount = 3,
        BathroomCount = 2,
        AreaSquareMeters = 145.50m
    };

    private static Property CreateExisting(Guid id, string? imageUrl = null) => new()
    {
        Id = id,
        Title = "Casa original",
        Description = "Descripción original.",
        Address = "Avenida Central 456",
        Price = 180000m,
        Status = PropertyStatus.Available,
        BedroomCount = 2,
        BathroomCount = 1,
        AreaSquareMeters = 90m,
        ImageUrl = imageUrl,
        CreatedAt = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc),
        UpdatedAt = new DateTime(2026, 2, 1, 10, 0, 0, DateTimeKind.Utc)
    };

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static async Task SeedAsync(AppDbContext context, Property property)
    {
        context.Properties.Add(property);
        await context.SaveChangesAsync();
    }

    private static IFormFile CreateImage(string fileName, string contentType)
    {
        byte[] bytes = contentType == "image/png"
            ? [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x01, 0x02]
            : [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x01, 0x02];
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

        public List<string> RemovedUrls { get; } = [];

        public string StoredUrl { get; set; } = "/assets/properties/generated.png";

        public Exception? StoreException { get; set; }

        public Exception? RemoveException { get; set; }

        public bool IgnoreCancellation { get; set; }

        public Action? OnStore { get; set; }

        public Task<string> StoreAsync(IFormFile image, CancellationToken cancellationToken)
        {
            StoreCalls++;
            if (!IgnoreCancellation)
                cancellationToken.ThrowIfCancellationRequested();
            if (StoreException is not null)
                throw StoreException;
            OnStore?.Invoke();
            return Task.FromResult(StoredUrl);
        }

        public Task RemoveAsync(string imageUrl, CancellationToken cancellationToken)
        {
            RemoveCalls++;
            RemovedUrls.Add(imageUrl);
            if (RemoveException is not null)
                throw RemoveException;
            return Task.CompletedTask;
        }
    }
}
