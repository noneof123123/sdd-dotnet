using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using RealtorWeb.Components.Pages;
using RealtorWeb.Features.Properties;

namespace RealtorWebTests.Components;

public class PropertyDetailsTests
{
    [Fact]
    public void RenderShowsFullDetailsAndBackLinkToOriginPage()
    {
        using var context = new BunitContext();
        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var service = new StubCatalogService
        {
            DetailsHandler = (requestedId, page, _) =>
                Task.FromResult(CatalogTestData.Details(id: requestedId, page: page))
        };
        context.Services.AddSingleton<IPropertyCatalogService>(service);
        var navigation = context.Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo(navigation.GetUriWithQueryParameter("page", "2"));

        var cut = context.Render<PropertyDetails>(parameters => parameters.Add(item => item.Id, id));

        cut.WaitForAssertion(() => Assert.Contains("Piso luminoso", cut.Markup));
        Assert.Contains("Descripción completa del inmueble.", cut.Markup);
        Assert.Contains("Calle Mayor 1, Madrid", cut.Markup);
        Assert.Contains("1.250,00", cut.Markup);
        Assert.Contains("Disponible", cut.Markup);
        Assert.Contains("3 dormitorios", cut.Markup);
        Assert.Contains("2 baños", cut.Markup);
        Assert.Contains("95 m²", cut.Markup);
        Assert.Contains("14/05/2026 10:30", cut.Markup);
        Assert.Contains("20/05/2026 12:45", cut.Markup);
        Assert.Contains("href=\"/?page=2\"", cut.Markup);
        Assert.DoesNotContain("CreatedAt", cut.Markup);
        Assert.DoesNotContain("sort=", cut.Markup);
    }

    [Fact]
    public void RenderWithoutImageShowsAccessiblePlaceholder()
    {
        using var context = new BunitContext();
        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var service = new StubCatalogService
        {
            DetailsHandler = (requestedId, page, _) =>
                Task.FromResult(CatalogTestData.Details(id: requestedId, page: page, imageUrl: null))
        };
        context.Services.AddSingleton<IPropertyCatalogService>(service);

        var cut = context.Render<PropertyDetails>(parameters => parameters.Add(item => item.Id, id));

        cut.WaitForAssertion(() => Assert.Contains("Imagen no disponible para Piso luminoso", cut.Markup));
        Assert.Contains("role=\"img\"", cut.Markup);
        Assert.DoesNotContain("<img", cut.Markup);
    }

    [Fact]
    public void RenderMissingPropertyShowsNotFoundMessageAndBackLink()
    {
        using var context = new BunitContext();
        var id = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var service = new StubCatalogService
        {
            DetailsHandler = (_, page, _) =>
                Task.FromResult(CatalogTestData.Details(id: id, page: page, state: PresentationState.NoEncontrado))
        };
        context.Services.AddSingleton<IPropertyCatalogService>(service);
        var navigation = context.Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo(navigation.GetUriWithQueryParameter("page", "3"));

        var cut = context.Render<PropertyDetails>(parameters => parameters.Add(item => item.Id, id));

        cut.WaitForAssertion(() => Assert.Contains("No encontramos esta propiedad.", cut.Markup));
        Assert.Contains("href=\"/?page=3\"", cut.Markup);
        Assert.DoesNotContain("Piso luminoso", cut.Markup);
    }

    [Fact]
    public void RenderDirectRouteShowsSameInformationAndFirstPageBackLink()
    {
        using var context = new BunitContext();
        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var service = new StubCatalogService
        {
            DetailsHandler = (requestedId, page, _) =>
                Task.FromResult(CatalogTestData.Details(id: requestedId, page: page))
        };
        context.Services.AddSingleton<IPropertyCatalogService>(service);

        var cut = context.Render<PropertyDetails>(parameters => parameters.Add(item => item.Id, id));

        cut.WaitForAssertion(() => Assert.Contains("Piso luminoso", cut.Markup));
        Assert.Contains("Descripción completa del inmueble.", cut.Markup);
        Assert.Contains("Calle Mayor 1, Madrid", cut.Markup);
        Assert.Contains("1.250,00", cut.Markup);
        Assert.Contains("3 dormitorios", cut.Markup);
        Assert.Contains("2 baños", cut.Markup);
        Assert.Contains("95 m²", cut.Markup);
        Assert.Contains("14/05/2026 10:30", cut.Markup);
        Assert.Contains("href=\"/?page=1\"", cut.Markup);
    }

    [Fact]
    public void RenderMissingUpdatedDateShowsAvailabilityText()
    {
        using var context = new BunitContext();
        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var service = new StubCatalogService
        {
            DetailsHandler = (requestedId, page, _) =>
                Task.FromResult(CatalogTestData.Details(
                    id: requestedId,
                    page: page,
                    updatedAt: "No disponible"))
        };
        context.Services.AddSingleton<IPropertyCatalogService>(service);

        var cut = context.Render<PropertyDetails>(parameters => parameters.Add(item => item.Id, id));

        cut.WaitForAssertion(() => Assert.Contains("No disponible", cut.Markup));
        Assert.Contains("Última actualización", cut.Markup);
    }

    [Fact]
    public void RenderUnknownStatusShowsFallbackLabel()
    {
        using var context = new BunitContext();
        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var service = new StubCatalogService
        {
            DetailsHandler = (requestedId, page, _) =>
                Task.FromResult(CatalogTestData.Details(id: requestedId, page: page, status: "desconocido"))
        };
        context.Services.AddSingleton<IPropertyCatalogService>(service);

        var cut = context.Render<PropertyDetails>(parameters => parameters.Add(item => item.Id, id));

        cut.WaitForAssertion(() => Assert.Contains("Estado no disponible", cut.Markup));
        Assert.Contains("Piso luminoso", cut.Markup);
    }
}
