using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using RealtorApi.Domain.Properties;
using RealtorApi.Features.Properties.GetPropertyById;
using RealtorApi.Features.Properties.ListProperties;
using RealtorApi.Infrastructure.Persistence;
using RealtorApi.Infrastructure.Results;

namespace RealtorApiTests.Features.Properties.GetPropertyById;

public sealed class GetPropertyByIdHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithExistingProperty_ReturnsFullResponseWithoutTrackingOrWrites()
    {
        await using var context = CreateContext();
        var existing = CreateProperty(Guid.NewGuid(), "/images/photo.png");
        await SeedAsync(context, existing);
        context.ChangeTracker.Clear();
        var handler = CreateHandler(context);

        var result = await handler.HandleAsync(
            new GetPropertyByIdRequest { Id = existing.Id },
            CreateRequest("http", "localhost:5023"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var response = Assert.IsType<GetPropertyByIdResponse>(result.Value);
        Assert.Equal(existing.Id, response.Id);
        Assert.Equal(existing.Title, response.Title);
        Assert.Equal(existing.Description, response.Description);
        Assert.Equal(existing.Address, response.Address);
        Assert.Equal(existing.Price, response.Price);
        Assert.Equal(existing.Status.ToString(), response.Status);
        Assert.Equal(existing.BedroomCount, response.BedroomCount);
        Assert.Equal(existing.BathroomCount, response.BathroomCount);
        Assert.Equal(existing.AreaSquareMeters, response.AreaSquareMeters);
        Assert.Equal("http://localhost:5023/assets/properties/photo.png", response.ImageUrl);
        Assert.Equal(existing.CreatedAt, response.CreatedAt);
        Assert.Equal(existing.UpdatedAt, response.UpdatedAt);
        Assert.Empty(context.ChangeTracker.Entries());
        AssertUnchanged(existing, await ReloadAsync(context, existing.Id));
    }

    [Fact]
    public async Task HandleAsync_WithExistingPropertyWithoutImage_ReturnsEmptyImageUrl()
    {
        await using var context = CreateContext();
        var existing = CreateProperty(Guid.NewGuid(), null);
        existing.UpdatedAt = null;
        await SeedAsync(context, existing);
        context.ChangeTracker.Clear();

        var result = await CreateHandler(context).HandleAsync(
            new GetPropertyByIdRequest { Id = existing.Id },
            CreateRequest("https", "example.test"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(string.Empty, result.Value!.ImageUrl);
        Assert.Null(result.Value.UpdatedAt);
    }

    [Fact]
    public async Task HandleAsync_WhenPropertyMissing_ReturnsNotFoundWithoutSideEffects()
    {
        await using var context = CreateContext();
        var existing = CreateProperty(Guid.NewGuid(), "/images/photo.png");
        await SeedAsync(context, existing);
        context.ChangeTracker.Clear();
        var missingId = Guid.NewGuid();

        var result = await CreateHandler(context).HandleAsync(
            new GetPropertyByIdRequest { Id = missingId },
            CreateRequest("http", "localhost:5023"),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal("property_not_found", result.Error.Code);
        Assert.Empty(context.ChangeTracker.Entries());
        AssertUnchanged(existing, await ReloadAsync(context, existing.Id));
    }

    [Fact]
    public async Task HandleAsync_WhenCancelled_PropagatesWithoutSideEffects()
    {
        await using var context = CreateContext();
        var existing = CreateProperty(Guid.NewGuid(), "/images/photo.png");
        await SeedAsync(context, existing);
        context.ChangeTracker.Clear();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            CreateHandler(context).HandleAsync(
                new GetPropertyByIdRequest { Id = existing.Id },
                CreateRequest("http", "localhost:5023"),
                cancellation.Token));

        Assert.Empty(context.ChangeTracker.Entries());
        AssertUnchanged(existing, await ReloadAsync(context, existing.Id));
    }

    [Fact]
    public async Task HandleAsync_WhenContextFails_ReturnsInternalWithoutChanges()
    {
        await using var context = CreateContext();
        var existing = CreateProperty(Guid.NewGuid(), "/images/photo.png");
        await SeedAsync(context, existing);
        context.ChangeTracker.Clear();
        context.Dispose();

        var result = await CreateHandler(context).HandleAsync(
            new GetPropertyByIdRequest { Id = existing.Id },
            CreateRequest("http", "localhost:5023"),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Internal, result.Error!.Type);
        Assert.DoesNotContain("ObjectDisposed", result.Error.Detail, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task HandleAsync_UpdatedProperty_MatchesListHandlerAndReflectsCurrentState()
    {
        await using var context = CreateContext();
        var existing = CreateProperty(Guid.NewGuid(), "/images/old.png");
        await SeedAsync(context, existing);
        existing.Title = "Casa actualizada";
        existing.Status = PropertyStatus.Maintenance;
        existing.ImageUrl = "/images/updated.png";
        existing.UpdatedAt = new DateTime(2026, 11, 20, 12, 0, 0, DateTimeKind.Utc);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        var request = CreateRequest("http", "localhost:5023");

        var detail = await CreateHandler(context).HandleAsync(
            new GetPropertyByIdRequest { Id = existing.Id },
            request,
            CancellationToken.None);
        var list = await new ListPropertiesHandler(context).HandleAsync(
            new ListPropertiesRequest(),
            request,
            CancellationToken.None);

        Assert.True(detail.IsSuccess);
        var item = Assert.Single(list.Items);
        Assert.Equal(item.Id, detail.Value!.Id);
        Assert.Equal(item.Title, detail.Value.Title);
        Assert.Equal(item.Description, detail.Value.Description);
        Assert.Equal(item.Address, detail.Value.Address);
        Assert.Equal(item.Price, detail.Value.Price);
        Assert.Equal(item.Status, detail.Value.Status);
        Assert.Equal(item.BedroomCount, detail.Value.BedroomCount);
        Assert.Equal(item.BathroomCount, detail.Value.BathroomCount);
        Assert.Equal(item.AreaSquareMeters, detail.Value.AreaSquareMeters);
        Assert.Equal(item.ImageUrl, detail.Value.ImageUrl);
        Assert.Equal(existing.UpdatedAt, detail.Value.UpdatedAt);
    }

    private static GetPropertyByIdHandler CreateHandler(AppDbContext context) =>
        new(context, NullLogger<GetPropertyByIdHandler>.Instance);

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

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

    private static async Task SeedAsync(AppDbContext context, Property property)
    {
        context.Properties.Add(property);
        await context.SaveChangesAsync();
    }

    private static HttpRequest CreateRequest(string scheme, string host)
    {
        var context = new DefaultHttpContext();
        context.Request.Scheme = scheme;
        context.Request.Host = new HostString(host);
        return context.Request;
    }

    private static async Task<Property> ReloadAsync(AppDbContext context, Guid id) =>
        await context.Properties.AsNoTracking().SingleAsync(property => property.Id == id);

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
}
