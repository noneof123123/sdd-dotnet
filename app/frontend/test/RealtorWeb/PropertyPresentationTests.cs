using System.Net;
using PropertyState = RealtorWeb.Features.Properties.PresentationState;
using RealtorWeb.Features.Properties;
using Refit;

namespace RealtorWebTests;

public class PropertyPresentationTests
{
    [Fact]
    public void CatalogStatePrioritizesLoadingThenFailureThenContent()
    {
        Assert.Equal(PropertyState.Cargando, PropertyPresentation.CatalogState(true, true, true));
        Assert.Equal(PropertyState.Error, PropertyPresentation.CatalogState(false, false, true));
        Assert.Equal(PropertyState.Vacio, PropertyPresentation.CatalogState(false, true, false));
        Assert.Equal(PropertyState.Contenido, PropertyPresentation.CatalogState(false, true, true));
    }

    [Fact]
    public void DetailsStatePrioritizesLoadingThenNotFoundThenFailureThenContent()
    {
        Assert.Equal(PropertyState.Cargando, PropertyPresentation.DetailsState(true, false, true));
        Assert.Equal(PropertyState.NoEncontrado, PropertyPresentation.DetailsState(false, true, true));
        Assert.Equal(PropertyState.Error, PropertyPresentation.DetailsState(false, false, false));
        Assert.Equal(PropertyState.Contenido, PropertyPresentation.DetailsState(false, true, false));
    }

    [Theory]
    [InlineData(PropertyState.Cargando, false, PropertyPresentation.LoadingCatalogMessage)]
    [InlineData(PropertyState.Cargando, true, PropertyPresentation.LoadingDetailsMessage)]
    [InlineData(PropertyState.Vacio, false, PropertyPresentation.EmptyMessage)]
    [InlineData(PropertyState.Error, false, PropertyPresentation.ErrorMessage)]
    [InlineData(PropertyState.NoEncontrado, true, PropertyPresentation.NotFoundMessage)]
    public void MessageForReturnsExpectedSpanishMessage(PropertyState state, bool isDetails, string expected)
    {
        Assert.Equal(expected, PropertyPresentation.MessageFor(state, isDetails));
    }

    [Theory]
    [InlineData(0, "0 propiedades")]
    [InlineData(1, "1 propiedad")]
    [InlineData(1234, "1.234 propiedades")]
    public void TotalItemsTextUsesSpanishFormat(int totalItems, string expected)
    {
        var catalog = CatalogTestData.CatalogWith(totalItems: totalItems);

        Assert.Equal(expected, catalog.TotalItemsText);
    }

    [Fact]
    public async Task GetCatalogAsyncNormalizesMetadataAndUsesEffectivePage()
    {
        var api = new FakePropertiesApi
        {
            ListHandler = (page, pageSize) =>
            {
                Assert.Equal(7, page);
                Assert.Equal(CatalogNavigation.PageSize, pageSize);
                return Task.FromResult(Success(new ListPropertiesResponse
                {
                    Page = 3,
                    PageSize = CatalogNavigation.PageSize,
                    TotalItems = -5,
                    TotalPages = 0,
                    HasPrevious = true,
                    HasNext = false,
                    Items = []
                }));
            }
        };
        var service = new PropertyCatalogService(api);

        var result = await service.GetCatalogAsync(7, CancellationToken.None);

        Assert.Equal(3, result.Page);
        Assert.Equal(0, result.TotalItems);
        Assert.Equal(1, result.TotalPages);
        Assert.Equal("Página 3 de 1", result.SummaryText);
        Assert.Equal("0 propiedades", result.TotalItemsText);
        Assert.Equal("/?page=2", result.PreviousUrl);
        Assert.Null(result.NextUrl);
        Assert.Equal(PropertyState.Vacio, result.State);
    }

    [Fact]
    public async Task GetCatalogAsyncMapsExpectedFailureToErrorState()
    {
        var api = new FakePropertiesApi
        {
            ListHandler = (_, _) => throw new HttpRequestException("fallo de red")
        };
        var service = new PropertyCatalogService(api);

        var result = await service.GetCatalogAsync(2, CancellationToken.None);

        Assert.Equal(PropertyState.Error, result.State);
        Assert.Equal(PropertyPresentation.ErrorMessage, result.Message);
        Assert.Equal("/?page=2", result.RetryUrl);
        Assert.False(result.HasPrevious);
        Assert.False(result.HasNext);
        Assert.Equal("0 propiedades", result.TotalItemsText);
    }

    private static ApiResponse<T> Success<T>(T content)
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            RequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://example.test/api/properties")
        };
        return new ApiResponse<T>(response, content, new RefitSettings());
    }

    private sealed class FakePropertiesApi : IPropertiesApi
    {
        public Func<int, int, Task<ApiResponse<ListPropertiesResponse>>> ListHandler { get; init; } =
            (_, _) => throw new NotSupportedException();

        public Task<ApiResponse<ListPropertiesResponse>> ListAsync(
            int page,
            int pageSize,
            CancellationToken cancellationToken) =>
            ListHandler(page, pageSize);

        public Task<ApiResponse<GetPropertyByIdResponse>> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }
}
