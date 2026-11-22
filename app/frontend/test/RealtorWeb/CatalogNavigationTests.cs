using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using RealtorWeb.Components.Pages;
using RealtorWeb.Features.Interactions;
using RealtorWeb.Features.Properties;

namespace RealtorWebTests;

public class CatalogNavigationTests
{
    [Theory]
    [InlineData(null, 1)]
    [InlineData("", 1)]
    [InlineData("   ", 1)]
    [InlineData("no-valido", 1)]
    [InlineData("0", 1)]
    [InlineData("-1", 1)]
    [InlineData("+2", 1)]
    [InlineData("999999999999999999999", 1)]
    [InlineData("7", 7)]
    public void SanitizePageReturnsOnlyPositiveIntegers(string? value, int expected)
    {
        Assert.Equal(expected, CatalogNavigation.SanitizePage(value));
    }

    [Fact]
    public void CatalogUrlUsesSanitizedPage()
    {
        Assert.Equal("/?page=1", CatalogNavigation.CatalogUrl(0));
        Assert.Equal("/?page=4", CatalogNavigation.CatalogUrl(4));
    }

    [Fact]
    public void DetailsUrlPreservesSanitizedPage()
    {
        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");

        Assert.Equal(
            "/properties/11111111-1111-1111-1111-111111111111?page=2",
            CatalogNavigation.DetailsUrl(id, 2));
        Assert.Equal(
            "/properties/11111111-1111-1111-1111-111111111111?page=1",
            CatalogNavigation.DetailsUrl(id, -3));
    }

    [Fact]
    public void PaginationLinksAppearOnlyWhenAllowed()
    {
        Assert.Null(CatalogNavigation.PreviousUrl(1, false));
        Assert.Null(CatalogNavigation.NextUrl(3, false));
        Assert.Equal("/?page=1", CatalogNavigation.PreviousUrl(2, true));
        Assert.Equal("/?page=4", CatalogNavigation.NextUrl(3, true));
    }

    [Fact]
    public void RetryAndBackLinksUseSanitizedPage()
    {
        Assert.Equal("/?page=2", CatalogNavigation.RetryCatalogUrl(2));
        Assert.Equal(
            "/properties/11111111-1111-1111-1111-111111111111?page=2",
            CatalogNavigation.RetryDetailsUrl(Guid.Parse("11111111-1111-1111-1111-111111111111"), 2));
        Assert.Equal("/?page=2", CatalogNavigation.BackUrl(2));
    }

    [Fact]
    public void EffectivePageSummaryOverridesOverflowRequest()
    {
        using var context = new BunitContext();
        context.Services.AddSingleton(new InteractionContext());
        var requestedPage = 0;
        var service = new EffectivePageCatalogService(page =>
        {
            requestedPage = page;
            return CatalogTestData.CatalogWith(
                page: 3,
                totalItems: 36,
                totalPages: 3,
                hasPrevious: true,
                hasNext: false);
        });
        context.Services.AddSingleton<IPropertyCatalogService>(service);

        var navigation = context.Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo(navigation.GetUriWithQueryParameter("page", "999999"));

        var cut = context.Render<Home>();

        cut.WaitForAssertion(() => Assert.Equal(999999, requestedPage));
        Assert.Contains("Página 3 de 3", cut.Markup);
        Assert.Contains("36 propiedades", cut.Markup);
        Assert.DoesNotContain("No encontramos esta propiedad", cut.Markup);
    }

    private sealed class EffectivePageCatalogService(Func<int, PropertyCatalogViewModel> catalogFactory)
        : IPropertyCatalogService
    {
        public Task<PropertyCatalogViewModel> GetCatalogAsync(int page, CancellationToken cancellationToken) =>
            Task.FromResult(catalogFactory(page));

        public Task<PropertyDetailsViewModel> GetDetailsAsync(
            Guid id,
            int page,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }
}
