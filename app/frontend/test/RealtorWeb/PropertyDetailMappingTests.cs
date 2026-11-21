using RealtorWeb.Features.Properties;
using PropertyState = RealtorWeb.Features.Properties.PresentationState;

namespace RealtorWebTests;

public class PropertyDetailMappingTests
{
    [Fact]
    public async Task GetDetailsAsyncMapsFullResponseWithSpanishFormats()
    {
        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var api = new FakeDetailApi
        {
            GetHandler = (requestedId, _) =>
                Task.FromResult(DetailTestData.Success(DetailTestData.Response(id: requestedId)))
        };
        var service = new PropertyCatalogService(api);

        using var cancellation = new CancellationTokenSource();
        var result = await service.GetDetailsAsync(id, 0, cancellation.Token);

        Assert.Equal(id, api.RequestedId);
        Assert.Equal(cancellation.Token, api.RequestedToken);
        Assert.Equal(PropertyState.Contenido, result.State);
        Assert.Equal("Piso luminoso", result.Title);
        Assert.Equal("Descripción completa del inmueble.", result.Description);
        Assert.Equal("Calle Mayor 1, Madrid", result.Address);
        Assert.Equal("1.250,00", result.Price);
        Assert.Equal("Disponible", result.StatusLabel);
        Assert.Equal(3, result.BedroomCount);
        Assert.Equal(2, result.BathroomCount);
        Assert.Equal(95, result.AreaSquareMeters);
        Assert.Equal("/images/piso.jpg", result.ImageUrl);
        Assert.Equal("Imagen de Piso luminoso", result.ImageAlt);
        Assert.Equal("14/05/2026 10:30", result.CreatedAt);
        Assert.Equal("20/05/2026 12:45", result.UpdatedAt);
        Assert.Equal("/?page=1", result.BackUrl);
        Assert.Null(result.RetryUrl);
    }

    [Fact]
    public async Task GetDetailsAsyncMapsMissingImageAndUpdatedDate()
    {
        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var api = new FakeDetailApi
        {
            GetHandler = (_, _) =>
                Task.FromResult(DetailTestData.Success(DetailTestData.Response(
                    imageUrl: "   ",
                    status: "otro",
                    hasUpdatedAt: false)))
        };
        var service = new PropertyCatalogService(api);

        var result = await service.GetDetailsAsync(id, 4, CancellationToken.None);

        Assert.Null(result.ImageUrl);
        Assert.Equal("Imagen no disponible para Piso luminoso", result.ImageAlt);
        Assert.Equal("No disponible", result.UpdatedAt);
        Assert.Equal("Estado no disponible", result.StatusLabel);
        Assert.Equal("/?page=4", result.BackUrl);
    }
}
