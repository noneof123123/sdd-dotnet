using Bunit;
using Microsoft.Extensions.DependencyInjection;
using RealtorWeb.Components.Pages;
using RealtorWeb.Features.Interactions;
using RealtorWeb.Features.Properties;

namespace RealtorWebTests.Components;

public class PropertyDetailsAccessibilityTests
{
    [Fact]
    public void RenderContentHasAccessibleHeadingSectionAndImageAlternative()
    {
        using var context = new BunitContext();
        context.Services.AddSingleton(new InteractionContext());
        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        context.Services.AddSingleton<IPropertyCatalogService>(new StubCatalogService
        {
            DetailsHandler = (requestedId, page, _) =>
                Task.FromResult(CatalogTestData.Details(id: requestedId, page: page))
        });

        var cut = context.Render<PropertyDetails>(parameters => parameters.Add(item => item.Id, id));

        cut.WaitForAssertion(() => Assert.Single(cut.FindAll("h1")));
        Assert.Contains("aria-labelledby=\"detalle-titulo\"", cut.Markup);
        Assert.Contains("id=\"detalle-titulo\"", cut.Markup);
        Assert.Contains("alt=\"Imagen de Piso luminoso\"", cut.Markup);
        Assert.Contains("aria-hidden=\"true\"", cut.Markup);
    }

    [Theory]
    [InlineData(PresentationState.Cargando)]
    [InlineData(PresentationState.NoEncontrado)]
    [InlineData(PresentationState.Error)]
    public void RenderNonContentStatesKeepAccessibleHeading(PresentationState state)
    {
        using var context = new BunitContext();
        context.Services.AddSingleton(new InteractionContext());
        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        context.Services.AddSingleton<IPropertyCatalogService>(new StubCatalogService
        {
            DetailsHandler = (requestedId, page, _) =>
                Task.FromResult(CatalogTestData.Details(id: requestedId, page: page, state: state))
        });

        var cut = context.Render<PropertyDetails>(parameters => parameters.Add(item => item.Id, id));

        cut.WaitForAssertion(() => Assert.Single(cut.FindAll("h1")));
        Assert.Contains("id=\"detalle-titulo\"", cut.Markup);
        Assert.Contains("aria-labelledby=\"detalle-titulo\"", cut.Markup);
        Assert.DoesNotContain("Piso luminoso", cut.Markup);
    }
}
