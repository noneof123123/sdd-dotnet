using PropertyState = RealtorWeb.Features.Properties.PresentationState;
using RealtorWeb.Features.Properties;

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
}
