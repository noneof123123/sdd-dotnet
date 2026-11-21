using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using RealtorWeb.Components.Pages;
using RealtorWeb.Features.Properties;

namespace RealtorWebTests.Components;

public class PropertyDetailsBackLinkTests
{
    [Theory]
    [InlineData("2", "/?page=2")]
    [InlineData("0", "/?page=1")]
    [InlineData("no-valido", "/?page=1")]
    [InlineData("999999", "/?page=999999")]
    public void RenderBackLinkUsesSanitizedContext(string page, string expectedHref)
    {
        using var context = new BunitContext();
        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var service = new StubCatalogService
        {
            DetailsHandler = (requestedId, requestedPage, _) =>
                Task.FromResult(CatalogTestData.Details(id: requestedId, page: requestedPage))
        };
        context.Services.AddSingleton<IPropertyCatalogService>(service);
        var navigation = context.Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo(navigation.GetUriWithQueryParameter("page", page));

        var cut = context.Render<PropertyDetails>(parameters => parameters.Add(item => item.Id, id));

        cut.WaitForAssertion(() => Assert.Contains($"href=\"{expectedHref}\"", cut.Markup));
        Assert.Contains("Volver al catálogo", cut.Markup);
    }
}
