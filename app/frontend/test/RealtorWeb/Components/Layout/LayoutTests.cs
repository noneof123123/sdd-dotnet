using Bunit;
using RealtorWeb.Components.Layout;

namespace RealtorWebTests.Components.Layout;

public class LayoutTests
{
    [Fact]
    public void NavMenuShowsAccessibleBrandAndCatalogLink()
    {
        using var context = new BunitContext();

        var cut = context.Render<NavMenu>();

        Assert.Contains("aria-label=\"Realtor, ir al catálogo\"", cut.Markup);
        Assert.Contains("aria-label=\"Navegación principal\"", cut.Markup);
        Assert.Contains("href=\"/\"", cut.Markup);
        Assert.Contains("Catálogo", cut.Markup);
        Assert.Contains("aria-hidden=\"true\"", cut.Markup);
    }

    [Fact]
    public void MainLayoutRendersCommonMainLandmark()
    {
        using var context = new BunitContext();

        var cut = context.Render<MainLayout>(parameters =>
            parameters.Add(layout => layout.Body, builder => builder.AddContent(0, "Contenido de prueba")));

        Assert.Contains("<main id=\"contenido\"", cut.Markup);
        Assert.Contains("Contenido de prueba", cut.Markup);
    }
}
