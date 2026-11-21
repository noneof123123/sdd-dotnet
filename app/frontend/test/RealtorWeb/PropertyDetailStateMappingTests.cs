using System.Net;
using RealtorWeb.Features.Properties;
using PropertyState = RealtorWeb.Features.Properties.PresentationState;

namespace RealtorWebTests;

public class PropertyDetailStateMappingTests
{
    [Fact]
    public async Task GetDetailsAsyncMapsNotFoundWithoutPartialData()
    {
        var id = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var api = new FakeDetailApi
        {
            GetHandler = (_, _) => Task.FromResult(DetailTestData.Status(HttpStatusCode.NotFound))
        };
        var service = new PropertyCatalogService(api);

        var result = await service.GetDetailsAsync(id, 2, CancellationToken.None);

        Assert.Equal(PropertyState.NoEncontrado, result.State);
        Assert.Equal(PropertyPresentation.NotFoundMessage, result.Message);
        Assert.Equal("/?page=2", result.BackUrl);
        Assert.Equal("/?page=2", result.RetryUrl);
        Assert.Equal(string.Empty, result.Title);
        Assert.Equal(string.Empty, result.Description);
        Assert.Equal(string.Empty, result.Price);
    }

    [Fact]
    public async Task GetDetailsAsyncMapsApiFailureToErrorWithRetry()
    {
        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var api = new FakeDetailApi
        {
            GetHandler = (_, _) => Task.FromResult(DetailTestData.Status(HttpStatusCode.InternalServerError))
        };
        var service = new PropertyCatalogService(api);

        var result = await service.GetDetailsAsync(id, 2, CancellationToken.None);

        Assert.Equal(PropertyState.Error, result.State);
        Assert.Equal(PropertyPresentation.ErrorMessage, result.Message);
        Assert.Equal(
            "/properties/11111111-1111-1111-1111-111111111111?page=2",
            result.RetryUrl);
    }

    [Fact]
    public async Task GetDetailsAsyncMapsNetworkFailureToError()
    {
        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var api = new FakeDetailApi
        {
            GetHandler = (_, _) => throw new HttpRequestException("fallo de red")
        };
        var service = new PropertyCatalogService(api);

        var result = await service.GetDetailsAsync(id, 3, CancellationToken.None);

        Assert.Equal(PropertyState.Error, result.State);
        Assert.Equal(
            "/properties/11111111-1111-1111-1111-111111111111?page=3",
            result.RetryUrl);
    }

    [Fact]
    public async Task GetDetailsAsyncPropagatesRequestedCancellation()
    {
        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var api = new FakeDetailApi
        {
            GetHandler = (_, token) => throw new OperationCanceledException(token)
        };
        var service = new PropertyCatalogService(api);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.GetDetailsAsync(id, 1, cancellation.Token));
    }
}
