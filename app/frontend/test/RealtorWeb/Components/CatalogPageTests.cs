using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using RealtorWeb.Components.Pages;
using RealtorWeb.Features.Properties;

namespace RealtorWebTests.Components;

public class CatalogPageTests
{
    [Fact]
    public void RenderShowsCatalogCardsPaginationAndPreservedPage()
    {
        using var context = new BunitContext();
        var card = CatalogTestData.Card(page: 2);
        var service = new StubCatalogService
        {
            CatalogHandler = (_, _) => Task.FromResult(CatalogTestData.Catalog(card))
        };
        context.Services.AddSingleton<IPropertyCatalogService>(service);

        var navigation = context.Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo(navigation.GetUriWithQueryParameter("page", "2"));

        var cut = context.Render<Home>();

        cut.WaitForAssertion(() => Assert.Contains("Piso luminoso", cut.Markup));
        Assert.Contains("Página 2 de 3", cut.Markup);
        Assert.Contains("href=\"/?page=1\"", cut.Markup);
        Assert.Contains("href=\"/?page=3\"", cut.Markup);
        Assert.Contains("href=\"/properties/11111111-1111-1111-1111-111111111111?page=2\"", cut.Markup);
        Assert.Contains("Disponible", cut.Markup);
        Assert.DoesNotContain("Available", cut.Markup);
    }

    [Fact]
    public void RenderSanitizesInvalidPageBeforeConsultingService()
    {
        using var context = new BunitContext();
        var consultedPage = 0;
        var service = new StubCatalogService
        {
            CatalogHandler = (page, _) =>
            {
                consultedPage = page;
                return Task.FromResult(CatalogTestData.Catalog());
            }
        };
        context.Services.AddSingleton<IPropertyCatalogService>(service);

        var navigation = context.Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo(navigation.GetUriWithQueryParameter("page", "no-valido"));

        var cut = context.Render<Home>();

        cut.WaitForAssertion(() => Assert.Equal(1, consultedPage));
    }

    [Fact]
    public void RenderShowsTotalAndAccessiblePagination()
    {
        using var context = new BunitContext();
        var service = new StubCatalogService
        {
            CatalogHandler = (_, _) => Task.FromResult(CatalogTestData.CatalogWith(
                totalItems: 5,
                items: new[] { CatalogTestData.Card() }))
        };
        context.Services.AddSingleton<IPropertyCatalogService>(service);

        var cut = context.Render<Home>();

        cut.WaitForAssertion(() => Assert.Contains("5 propiedades", cut.Markup));
        Assert.Contains("Página 2 de 3", cut.Markup);
        Assert.Contains("aria-label=\"Paginación del catálogo\"", cut.Markup);
        Assert.Contains(">Anterior</a>", cut.Markup);
        Assert.Contains(">Siguiente</a>", cut.Markup);
        Assert.DoesNotContain("No encontramos esta propiedad", cut.Markup);
    }

    [Fact]
    public void RenderShowsDisabledControlsWhenNoAdjacentPages()
    {
        using var context = new BunitContext();
        var service = new StubCatalogService
        {
            CatalogHandler = (_, _) => Task.FromResult(CatalogTestData.CatalogWith(
                page: 1,
                totalItems: 1,
                totalPages: 1,
                hasPrevious: false,
                hasNext: false,
                items: new[] { CatalogTestData.Card() }))
        };
        context.Services.AddSingleton<IPropertyCatalogService>(service);

        var cut = context.Render<Home>();

        cut.WaitForAssertion(() => Assert.Contains("aria-disabled=\"true\"", cut.Markup));
        Assert.Contains("Página anterior no disponible", cut.Markup);
        Assert.Contains("Página siguiente no disponible", cut.Markup);
        Assert.DoesNotContain("href=\"/?page=0\"", cut.Markup);
        Assert.DoesNotContain("href=\"/?page=2\"", cut.Markup);
    }

    [Fact]
    public void RenderUsesEffectivePageFromService()
    {
        using var context = new BunitContext();
        var consultedPage = 0;
        var service = new StubCatalogService
        {
            CatalogHandler = (page, _) =>
            {
                consultedPage = page;
                return Task.FromResult(CatalogTestData.CatalogWith(
                    page: 3,
                    totalItems: 36,
                    totalPages: 3,
                    hasPrevious: true,
                    hasNext: false,
                    items: new[] { CatalogTestData.Card(page: 3) }));
            }
        };
        context.Services.AddSingleton<IPropertyCatalogService>(service);

        var navigation = context.Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo(navigation.GetUriWithQueryParameter("page", "999999"));

        var cut = context.Render<Home>();

        cut.WaitForAssertion(() => Assert.Equal(999999, consultedPage));
        Assert.Contains("Página 3 de 3", cut.Markup);
        Assert.Contains("36 propiedades", cut.Markup);
        Assert.Contains("aria-disabled=\"true\"", cut.Markup);
        Assert.DoesNotContain("href=\"/?page=4\"", cut.Markup);
    }

    [Fact]
    public void RenderEmptyStateHidesPagination()
    {
        using var context = new BunitContext();
        var service = new StubCatalogService
        {
            CatalogHandler = (_, _) => Task.FromResult(CatalogTestData.CatalogWith(
                state: PresentationState.Vacio,
                items: Array.Empty<PropertyCardViewModel>()))
        };
        context.Services.AddSingleton<IPropertyCatalogService>(service);

        var cut = context.Render<Home>();

        cut.WaitForAssertion(() => Assert.Contains(PropertyPresentation.EmptyMessage, cut.Markup));
        Assert.DoesNotContain("class=\"pagination\"", cut.Markup);
        Assert.DoesNotContain("catalog-summary", cut.Markup);
    }

    [Fact]
    public void RenderErrorStateShowsRetryAndHidesPagination()
    {
        using var context = new BunitContext();
        var service = new StubCatalogService
        {
            CatalogHandler = (_, _) => Task.FromResult(CatalogTestData.CatalogWith(
                page: 2,
                state: PresentationState.Error,
                retryUrl: "/?page=2",
                items: Array.Empty<PropertyCardViewModel>()))
        };
        context.Services.AddSingleton<IPropertyCatalogService>(service);

        var cut = context.Render<Home>();

        cut.WaitForAssertion(() => Assert.Contains(PropertyPresentation.ErrorMessage, cut.Markup));
        Assert.Contains("Reintentar", cut.Markup);
        Assert.Contains("href=\"/?page=2\"", cut.Markup);
        Assert.DoesNotContain("class=\"pagination\"", cut.Markup);
    }
}
