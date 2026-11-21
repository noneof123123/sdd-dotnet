using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using RealtorWeb.Components.Pages;
using RealtorWeb.Features.Properties;

namespace RealtorWebTests;

public class PropertyDetailNavigationTests
{
    [Theory]
    [InlineData(null, 1, "/?page=1")]
    [InlineData("0", 1, "/?page=1")]
    [InlineData("no-valido", 1, "/?page=1")]
    [InlineData("2", 2, "/?page=2")]
    [InlineData("999999", 999999, "/?page=999999")]
    public void DetailPassesSanitizedPageAndPreservesOverflowContext(string? page, int expectedPage, string expectedBackUrl)
    {
        using var context = new BunitContext();
        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var capturedPage = 0;
        var service = new StubCatalogService
        {
            DetailsHandler = (requestedId, requestedPage, _) =>
            {
                capturedPage = requestedPage;
                return Task.FromResult(CatalogTestData.Details(id: requestedId, page: requestedPage));
            }
        };
        context.Services.AddSingleton<IPropertyCatalogService>(service);
        var navigation = context.Services.GetRequiredService<NavigationManager>();
        if (page is not null)
        {
            navigation.NavigateTo(navigation.GetUriWithQueryParameter("page", page));
        }

        var cut = context.Render<PropertyDetails>(parameters => parameters.Add(item => item.Id, id));

        cut.WaitForAssertion(() => Assert.Equal(expectedPage, capturedPage));
        Assert.Equal(expectedBackUrl, CatalogNavigation.BackUrl(capturedPage));
    }

    [Fact]
    public void RetryDetailsUrlKeepsIdentifierAndContext()
    {
        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");

        Assert.Equal(
            "/properties/11111111-1111-1111-1111-111111111111?page=2",
            CatalogNavigation.RetryDetailsUrl(id, 2));
        Assert.Equal(
            "/properties/11111111-1111-1111-1111-111111111111?page=1",
            CatalogNavigation.RetryDetailsUrl(id, 0));
    }
}
