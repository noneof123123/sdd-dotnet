using Bunit;
using RealtorWeb.Components.Properties;
using RealtorWeb.Features.Properties;

namespace RealtorWebTests.Components;

public class PropertyCardTests
{
    [Fact]
    public void RenderWithImageShowsAllRequiredFieldsAndSpanishStatus()
    {
        using var context = new BunitContext();
        var card = CatalogTestData.Card();

        var cut = context.Render<PropertyCard>(parameters => parameters.Add(item => item.Property, card));

        Assert.Contains("Piso luminoso", cut.Markup);
        Assert.Contains("1.250,00", cut.Markup);
        Assert.Contains("Calle Mayor 1, Madrid", cut.Markup);
        Assert.Contains("3 dormitorios", cut.Markup);
        Assert.Contains("2 baños", cut.Markup);
        Assert.Contains("95 m²", cut.Markup);
        Assert.Contains("alt=\"Imagen de Piso luminoso\"", cut.Markup);
        Assert.Contains("Disponible", cut.Markup);
        Assert.DoesNotContain("Available", cut.Markup);
    }

    [Fact]
    public void RenderWithoutImageShowsAccessiblePlaceholder()
    {
        using var context = new BunitContext();
        var card = CatalogTestData.Card(imageUrl: null);

        var cut = context.Render<PropertyCard>(parameters => parameters.Add(item => item.Property, card));

        Assert.Contains("Imagen no disponible para Piso luminoso", cut.Markup);
        Assert.Contains("role=\"img\"", cut.Markup);
        Assert.DoesNotContain("<img", cut.Markup);
    }

    [Theory]
    [InlineData(PropertyStatus.Available, "Disponible")]
    [InlineData(PropertyStatus.Rented, "Alquilada")]
    [InlineData(PropertyStatus.Maintenance, "Mantenimiento")]
    [InlineData("Otro", "Estado no disponible")]
    public void RenderUsesTranslatedStatus(string status, string expectedLabel)
    {
        using var context = new BunitContext();
        var card = CatalogTestData.Card(status: status);

        var cut = context.Render<PropertyCard>(parameters => parameters.Add(item => item.Property, card));

        Assert.Contains(expectedLabel, cut.Markup);
        Assert.DoesNotContain(status, cut.Markup);
    }
}
