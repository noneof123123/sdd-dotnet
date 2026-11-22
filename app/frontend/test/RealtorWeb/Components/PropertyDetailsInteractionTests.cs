using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using RealtorWeb.Components.Pages;
using RealtorWeb.Features.Interactions;
using RealtorWeb.Features.Properties;

namespace RealtorWebTests.Components;

public class PropertyDetailsInteractionTests
{
    [Fact]
    public void RenderUpdatesDetailContextAndPreservesReturnUrl()
    {
        using var context = new BunitContext();
        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var service = new StubCatalogService
        {
            DetailsHandler = (requestedId, page, _) =>
                Task.FromResult(CatalogTestData.Details(id: requestedId, page: page))
        };
        var interaction = new InteractionContext();
        context.Services.AddSingleton<IPropertyCatalogService>(service);
        context.Services.AddSingleton(interaction);
        var navigation = context.Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo(navigation.GetUriWithQueryParameter("page", "2"));

        var cut = context.Render<PropertyDetails>(parameters => parameters.Add(item => item.Id, id));

        cut.WaitForAssertion(() => Assert.Contains("Piso luminoso", cut.Markup));
        Assert.Equal(InteractiveView.Detalle, interaction.VistaActual);
        Assert.Equal(id, interaction.PropiedadId);
        Assert.Equal(2, interaction.PaginaSaneada);
        Assert.Equal("/?page=2", interaction.DireccionRetorno);
        Assert.Contains("href=\"/?page=2\"", cut.Markup);
    }

    [Fact]
    public void RenderDirectDetailUsesDefaultCatalogReturnUrl()
    {
        using var context = new BunitContext();
        var id = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var service = new StubCatalogService
        {
            DetailsHandler = (requestedId, _, _) =>
                Task.FromResult(CatalogTestData.Details(id: requestedId))
        };
        var interaction = new InteractionContext();
        context.Services.AddSingleton<IPropertyCatalogService>(service);
        context.Services.AddSingleton(interaction);

        var cut = context.Render<PropertyDetails>(parameters => parameters.Add(item => item.Id, id));

        cut.WaitForAssertion(() => Assert.Contains("Piso luminoso", cut.Markup));
        Assert.Equal(InteractiveView.Detalle, interaction.VistaActual);
        Assert.Equal(id, interaction.PropiedadId);
        Assert.Equal(1, interaction.PaginaSaneada);
        Assert.Equal("/?page=1", interaction.DireccionRetorno);
    }

    [Fact]
    public void RenderShowsWaitingAndRetriesAfterUnavailableDetail()
    {
        using var context = new BunitContext();
        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var completion = new TaskCompletionSource<PropertyDetailsViewModel>(TaskCreationOptions.RunContinuationsAsynchronously);
        var attempts = 0;
        var service = new StubCatalogService
        {
            DetailsHandler = (requestedId, page, _) =>
            {
                attempts++;
                return attempts == 1
                    ? completion.Task
                    : Task.FromResult(CatalogTestData.Details(id: requestedId, page: page));
            }
        };
        var interaction = new InteractionContext();
        context.Services.AddSingleton<IPropertyCatalogService>(service);
        context.Services.AddSingleton(interaction);

        var cut = context.Render<PropertyDetails>(parameters => parameters.Add(item => item.Id, id));

        Assert.Contains(InteractionMessages.EsperandoDetalle, cut.Markup);
        Assert.Equal(InteractionStatus.Esperando, interaction.Estado);

        completion.SetResult(CatalogTestData.Details(
            id: id,
            state: PresentationState.Error,
            retryUrl: CatalogNavigation.RetryDetailsUrl(id, 1)));

        cut.WaitForAssertion(() => Assert.Contains("Reintentar", cut.Markup));
        Assert.Equal(InteractionStatus.NoDisponible, interaction.Estado);

        var reintentar = cut.Find("button");
        Assert.Equal("Reintentar", reintentar.TextContent);
        reintentar.Click();

        cut.WaitForAssertion(() => Assert.Contains("Piso luminoso", cut.Markup));
        Assert.Equal(InteractionStatus.Activa, interaction.Estado);
        Assert.Equal(2, attempts);
    }

    [Fact]
    public void RenderKeepsSingleHeadingSpanishTextsAndAccessibleIcons()
    {
        using var context = new BunitContext();
        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        context.Services.AddSingleton<IPropertyCatalogService>(new StubCatalogService());
        context.Services.AddSingleton(new InteractionContext());

        var cut = context.Render<PropertyDetails>(parameters => parameters.Add(item => item.Id, id));

        cut.WaitForAssertion(() => Assert.Contains("Piso luminoso", cut.Markup));
        Assert.Single(cut.FindAll("h1"));
        Assert.Contains("aria-labelledby=\"detalle-titulo\"", cut.Markup);
        Assert.Contains("aria-hidden=\"true\"", cut.Markup);
        Assert.Contains("Volver al catálogo", cut.Markup);
        Assert.DoesNotContain("CreatedAt", cut.Markup);
        Assert.All(cut.FindAll("a[href]"), link => Assert.False(string.IsNullOrWhiteSpace(link.GetAttribute("href"))));
    }
}
