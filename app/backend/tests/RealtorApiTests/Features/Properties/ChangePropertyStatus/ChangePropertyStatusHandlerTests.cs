using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RealtorApi.Domain.Properties;
using RealtorApi.Features.Properties.ChangePropertyStatus;
using RealtorApi.Infrastructure.Persistence;
using RealtorApi.Infrastructure.Results;

namespace RealtorApiTests.Features.Properties.ChangePropertyStatus;

public sealed class ChangePropertyStatusHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithDifferentStatus_ChangesOnlyStatusAndUpdatedAt()
    {
        var databaseName = Guid.NewGuid().ToString();
        await using var context = CreateContext(databaseName);
        var existing = CreateProperty(Guid.NewGuid(), "/images/photo.png");
        await SeedAsync(context, existing);
        context.ChangeTracker.Clear();
        var logger = new RecordingLogger();
        var handler = new ChangePropertyStatusHandler(context, logger);
        var before = DateTime.UtcNow;

        var result = await handler.HandleAsync(
            CreateRequest(existing.Id, "Rented"),
            CreateRequest("http", "localhost:5023"),
            CancellationToken.None);

        var after = DateTime.UtcNow;
        Assert.True(result.IsSuccess);
        var response = Assert.IsType<ChangePropertyStatusResponse>(result.Value);
        Assert.Equal(existing.Id, response.Id);
        Assert.Equal("Rented", response.Status);
        Assert.Equal("http://localhost:5023/assets/properties/photo.png", response.ImageUrl);
        AssertUnchangedExceptStatus(existing, response);

        var persisted = await ReloadAsync(databaseName, existing.Id);
        Assert.Equal(existing.Title, persisted.Title);
        Assert.Equal(existing.Description, persisted.Description);
        Assert.Equal(existing.Address, persisted.Address);
        Assert.Equal(existing.Price, persisted.Price);
        Assert.Equal(PropertyStatus.Rented, persisted.Status);
        Assert.Equal(existing.BedroomCount, persisted.BedroomCount);
        Assert.Equal(existing.BathroomCount, persisted.BathroomCount);
        Assert.Equal(existing.AreaSquareMeters, persisted.AreaSquareMeters);
        Assert.Equal(existing.ImageUrl, persisted.ImageUrl);
        Assert.Equal(existing.CreatedAt, persisted.CreatedAt);
        Assert.InRange(persisted.UpdatedAt!.Value, before, after);
        Assert.Equal(DateTimeKind.Utc, persisted.UpdatedAt.Value.Kind);
        Assert.Equal(1004, logger.EventId.Id);
        Assert.Equal(LogLevel.Information, logger.Level);
        Assert.Contains("Rented", logger.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task HandleAsync_WhenPropertyMissing_ReturnsNotFoundWithoutChanges()
    {
        var databaseName = Guid.NewGuid().ToString();
        await using var context = CreateContext(databaseName);
        var existing = CreateProperty(Guid.NewGuid(), "/images/photo.png");
        await SeedAsync(context, existing);
        context.ChangeTracker.Clear();
        var missingId = Guid.NewGuid();

        var result = await new ChangePropertyStatusHandler(context).HandleAsync(
            CreateRequest(missingId, "Maintenance"),
            CreateRequest("http", "localhost:5023"),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal("property_not_found", result.Error.Code);
        AssertUnchangedExceptStatus(existing, await ReloadAsync(databaseName, existing.Id));
    }

    [Fact]
    public async Task HandleAsync_WhenCancelledBeforeHandling_PropagatesWithoutChanges()
    {
        var databaseName = Guid.NewGuid().ToString();
        await using var context = CreateContext(databaseName);
        var existing = CreateProperty(Guid.NewGuid(), "/images/photo.png");
        await SeedAsync(context, existing);
        context.ChangeTracker.Clear();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            new ChangePropertyStatusHandler(context).HandleAsync(
                CreateRequest(existing.Id, "Rented"),
                CreateRequest("http", "localhost:5023"),
                cancellation.Token));

        AssertUnchangedExceptStatus(existing, await ReloadAsync(databaseName, existing.Id));
    }

    [Fact]
    public async Task HandleAsync_WhenPersistenceFails_RestoresStateAndReturnsInternal()
    {
        var databaseName = Guid.NewGuid().ToString();
        await using var context = CreateContext(databaseName);
        var existing = CreateProperty(Guid.NewGuid(), "/images/photo.png");
        await SeedAsync(context, existing);
        context.ChangeTracker.Clear();
        context.Dispose();

        var result = await new ChangePropertyStatusHandler(context).HandleAsync(
            CreateRequest(existing.Id, "Rented"),
            CreateRequest("http", "localhost:5023"),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Internal, result.Error!.Type);
        Assert.DoesNotContain("ObjectDisposed", result.Error.Detail, StringComparison.OrdinalIgnoreCase);
        var persisted = await ReloadAsync(databaseName, existing.Id);
        Assert.Equal(existing.Status, persisted.Status);
        Assert.Equal(existing.UpdatedAt, persisted.UpdatedAt);
    }

    [Fact]
    public async Task HandleAsync_TwoSequentialChanges_AppliesLastWriteWins()
    {
        var databaseName = Guid.NewGuid().ToString();
        await using var context = CreateContext(databaseName);
        var existing = CreateProperty(Guid.NewGuid(), "/images/photo.png");
        await SeedAsync(context, existing);
        context.ChangeTracker.Clear();
        var handler = new ChangePropertyStatusHandler(context);

        var first = await handler.HandleAsync(
            CreateRequest(existing.Id, "Rented"),
            CreateRequest("http", "localhost:5023"),
            CancellationToken.None);
        var second = await handler.HandleAsync(
            CreateRequest(existing.Id, "Maintenance"),
            CreateRequest("http", "localhost:5023"),
            CancellationToken.None);

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        Assert.NotEqual(ErrorType.Conflict, second.Error?.Type);
        var persisted = await ReloadAsync(databaseName, existing.Id);
        Assert.Equal(PropertyStatus.Maintenance, persisted.Status);
    }

    [Fact]
    public async Task HandleAsync_WithRepeatedStatus_DoesNotSaveAndKeepsUpdatedAt()
    {
        var databaseName = Guid.NewGuid().ToString();
        await using var context = CreateContext(databaseName);
        var existing = CreateProperty(Guid.NewGuid(), "/images/photo.png");
        await SeedAsync(context, existing);
        context.ChangeTracker.Clear();
        var logger = new RecordingLogger();

        var result = await new ChangePropertyStatusHandler(context, logger).HandleAsync(
            CreateRequest(existing.Id, "Available"),
            CreateRequest("http", "localhost:5023"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Available", result.Value!.Status);
        Assert.Equal(existing.UpdatedAt, result.Value.UpdatedAt);
        var persisted = await ReloadAsync(databaseName, existing.Id);
        Assert.Equal(existing.Status, persisted.Status);
        Assert.Equal(existing.UpdatedAt, persisted.UpdatedAt);
        Assert.Equal(1004, logger.EventId.Id);
    }

    [Theory]
    [InlineData("no-es-un-guid", "Rented")]
    [InlineData(null, "Rented")]
    public async Task HandleAsync_WithInvalidInput_ReturnsValidationErrorWithoutChanges(string? id, string status)
    {
        var databaseName = Guid.NewGuid().ToString();
        await using var context = CreateContext(databaseName);
        var existing = CreateProperty(Guid.NewGuid(), "/images/photo.png");
        await SeedAsync(context, existing);
        context.ChangeTracker.Clear();
        var request = new ChangePropertyStatusRequest
        {
            Id = id,
            Body = new ChangePropertyStatusBody { Status = status }
        };

        var result = await new ChangePropertyStatusHandler(context).HandleAsync(
            request,
            CreateRequest("http", "localhost:5023"),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
        AssertUnchangedExceptStatus(existing, await ReloadAsync(databaseName, existing.Id));
    }

    [Fact]
    public async Task HandleAsync_WithInvalidStatusText_ReturnsValidationErrorWithoutChanges()
    {
        var databaseName = Guid.NewGuid().ToString();
        await using var context = CreateContext(databaseName);
        var existing = CreateProperty(Guid.NewGuid(), "/images/photo.png");
        await SeedAsync(context, existing);
        context.ChangeTracker.Clear();

        var result = await new ChangePropertyStatusHandler(context).HandleAsync(
            CreateRequest(existing.Id, "rented"),
            CreateRequest("http", "localhost:5023"),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
        AssertUnchangedExceptStatus(existing, await ReloadAsync(databaseName, existing.Id));
    }

    private static ChangePropertyStatusRequest CreateRequest(Guid id, string status) => new()
    {
        Id = id.ToString(),
        Body = new ChangePropertyStatusBody { Status = status }
    };

    private static Property CreateProperty(Guid id, string? imageUrl) => new()
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

    private static AppDbContext CreateContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;
        return new AppDbContext(options);
    }

    private static async Task SeedAsync(AppDbContext context, Property property)
    {
        context.Properties.Add(property);
        await context.SaveChangesAsync();
    }

    private static async Task<Property> ReloadAsync(string databaseName, Guid id)
    {
        await using var context = CreateContext(databaseName);
        return await context.Properties.AsNoTracking().SingleAsync(candidate => candidate.Id == id);
    }

    private static HttpRequest CreateRequest(string scheme, string host)
    {
        var context = new DefaultHttpContext();
        context.Request.Scheme = scheme;
        context.Request.Host = new HostString(host);
        return context.Request;
    }

    private static void AssertUnchangedExceptStatus(Property expected, ChangePropertyStatusResponse actual)
    {
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Title, actual.Title);
        Assert.Equal(expected.Description, actual.Description);
        Assert.Equal(expected.Address, actual.Address);
        Assert.Equal(expected.Price, actual.Price);
        Assert.Equal(expected.BedroomCount, actual.BedroomCount);
        Assert.Equal(expected.BathroomCount, actual.BathroomCount);
        Assert.Equal(expected.AreaSquareMeters, actual.AreaSquareMeters);
        Assert.Equal(expected.CreatedAt, actual.CreatedAt);
    }

    private static void AssertUnchangedExceptStatus(Property expected, Property actual)
    {
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Title, actual.Title);
        Assert.Equal(expected.Description, actual.Description);
        Assert.Equal(expected.Address, actual.Address);
        Assert.Equal(expected.Price, actual.Price);
        Assert.Equal(expected.BedroomCount, actual.BedroomCount);
        Assert.Equal(expected.BathroomCount, actual.BathroomCount);
        Assert.Equal(expected.AreaSquareMeters, actual.AreaSquareMeters);
        Assert.Equal(expected.ImageUrl, actual.ImageUrl);
        Assert.Equal(expected.CreatedAt, actual.CreatedAt);
    }

    private sealed class RecordingLogger : ILogger<ChangePropertyStatusHandler>
    {
        public LogLevel Level { get; private set; }
        public EventId EventId { get; private set; }
        public string Message { get; private set; } = string.Empty;

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
            Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Level = logLevel;
            EventId = eventId;
            Message = formatter(state, exception);
        }
    }
}
