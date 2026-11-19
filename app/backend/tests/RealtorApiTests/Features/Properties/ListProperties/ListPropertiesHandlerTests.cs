using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RealtorApi.Domain.Properties;
using RealtorApi.Features.Properties.ListProperties;
using RealtorApi.Infrastructure.Persistence;

namespace RealtorApiTests.Features.Properties.ListProperties;

public sealed class ListPropertiesHandlerTests
{
    [Fact]
    public async Task HandleAsync_AppliesDefaults_OrderAndMetadata()
    {
        await using var context = CreateContext();
        context.Properties.AddRange(
            CreateProperty("Zulu", "B Street", 1),
            CreateProperty("Alpha", "Z Street", 2),
            CreateProperty("Alpha", "A Street", 3));
        await context.SaveChangesAsync();

        var handler = new ListPropertiesHandler(context);
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("example.test");

        var result = await handler.HandleAsync(
            new ListPropertiesRequest { Page = 1, PageSize = 6 },
            httpContext.Request,
            CancellationToken.None);

        Assert.Equal(["Alpha", "Alpha", "Zulu"], result.Items.Select(item => item.Title));
        Assert.Equal("A Street", result.Items[0].Address);
        Assert.Equal(3, result.TotalItems);
        Assert.Equal(1, result.TotalPages);
        Assert.False(result.HasNext);
        Assert.False(result.HasPrevious);
    }

    [Fact]
    public async Task HandleAsync_NormalizesPageOutsideRange()
    {
        await using var context = CreateContext();
        context.Properties.AddRange(
            CreateProperty("A", "1", 1),
            CreateProperty("B", "2", 2),
            CreateProperty("C", "3", 3));
        await context.SaveChangesAsync();

        var handler = new ListPropertiesHandler(context);
        var httpContext = new DefaultHttpContext();

        var result = await handler.HandleAsync(
            new ListPropertiesRequest { Page = 99, PageSize = 2 },
            httpContext.Request,
            CancellationToken.None);

        Assert.Equal(2, result.Page);
        Assert.Single(result.Items);
        Assert.False(result.HasNext);
        Assert.True(result.HasPrevious);
    }

    [Fact]
    public async Task HandleAsync_PropagatesCancellation()
    {
        await using var context = CreateContext();
        var handler = new ListPropertiesHandler(context);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var httpContext = new DefaultHttpContext();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            handler.HandleAsync(
                new ListPropertiesRequest { Page = 1, PageSize = 6 },
                httpContext.Request,
                cancellation.Token));
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static Property CreateProperty(string title, string address, int idSuffix)
    {
        return new Property
        {
            Id = Guid.Parse($"a1f0e2d4-7c81-4d12-9e61-0b7f0b1a10{idSuffix:D2}"),
            Title = title,
            Description = "Description",
            Address = address,
            Price = 100,
            Status = PropertyStatus.Available,
            BedroomCount = 2,
            BathroomCount = 1,
            AreaSquareMeters = 80,
            ImageUrl = $"/{idSuffix}.png",
            CreatedAt = DateTime.UtcNow
        };
    }
}
