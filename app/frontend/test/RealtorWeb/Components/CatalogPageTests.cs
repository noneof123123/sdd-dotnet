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
}
