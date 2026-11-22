using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using RealtorWeb.Components.Pages;
using RealtorWeb.Features.Interactions;
using RealtorWeb.Features.Properties;

namespace RealtorWebTests;

public class HomeInteractionTests
{
    [Fact]
    public void RenderUpdatesCatalogContextAndPreservesReturnUrl()
    {
        using var context = new BunitContext();
        var service = new StubCatalogService
        {
            CatalogHandler = (page, _) => Task.FromResult(CatalogTestData.CatalogWith(page: page, items: [CatalogTestData.Card(page: page)]))
        };
        var interaction = new InteractionContext();
        context.Services.AddSingleton<IPropertyCatalogService>(service);
        context.Services.AddSingleton(interaction);
        var navigation = context.Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo(navigation.GetUriWithQueryParameter("page", "2"));

        var cut = context.Render<Home>();

        cut.WaitForAssertion(() => Assert.Contains("Página 2 de 3", cut.Markup));
        Assert.Equal(InteractiveView.Catalogo, interaction.VistaActual);
        Assert.Equal("2", interaction.PaginaSolicitada);
        Assert.Equal(2, interaction.PaginaSaneada);
        Assert.Equal("/?page=2", interaction.DireccionRetorno);
        Assert.Contains("href=\"/properties/11111111-1111-1111-1111-111111111111?page=2\"", cut.Markup);
    }

    [Fact]
    public void RenderSanitizesInvalidPageInInteractionContext()
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
        var interaction = new InteractionContext();
        context.Services.AddSingleton<IPropertyCatalogService>(service);
        context.Services.AddSingleton(interaction);
        var navigation = context.Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo(navigation.GetUriWithQueryParameter("page", "no-valido"));

        var cut = context.Render<Home>();

        cut.WaitForAssertion(() => Assert.Equal(1, consultedPage));
        Assert.Equal("no-valido", interaction.PaginaSolicitada);
        Assert.Equal(1, interaction.PaginaSaneada);
        Assert.Equal("/?page=1", interaction.DireccionRetorno);
    }

    [Fact]
    public void RenderShowsWaitingAndRetriesAfterUnavailableCatalog()
    {
        using var context = new BunitContext();
        var completion = new TaskCompletionSource<PropertyCatalogViewModel>(TaskCreationOptions.RunContinuationsAsynchronously);
        var attempts = 0;
        var service = new StubCatalogService
        {
            CatalogHandler = (page, _) =>
            {
                attempts++;
                return attempts == 1
                    ? completion.Task
                    : Task.FromResult(CatalogTestData.CatalogWith(page: page, items: [CatalogTestData.Card(page: page)]));
            }
        };
        var interaction = new InteractionContext();
        context.Services.AddSingleton<IPropertyCatalogService>(service);
        context.Services.AddSingleton(interaction);

        var cut = context.Render<Home>();

        Assert.Contains(InteractionMessages.EsperandoCatalogo, cut.Markup);
        Assert.Equal(InteractionStatus.Esperando, interaction.Estado);

        completion.SetResult(CatalogTestData.CatalogWith(
            state: PresentationState.Error,
            message: PropertyPresentation.MessageFor(PresentationState.Error),
            retryUrl: CatalogNavigation.RetryCatalogUrl(1)));

        cut.WaitForAssertion(() => Assert.Contains("Reintentar", cut.Markup));
        Assert.Equal(InteractionStatus.NoDisponible, interaction.Estado);

        var reintentar = cut.Find("button");
        Assert.Equal("Reintentar", reintentar.TextContent);
        reintentar.Click();

        cut.WaitForAssertion(() => Assert.Equal(2, attempts));
        cut.WaitForAssertion(() => Assert.Contains("Piso luminoso", cut.Markup));
        Assert.Contains("Página 1 de 3", cut.Markup);
        Assert.Equal(InteractionStatus.Activa, interaction.Estado);
        Assert.Equal(2, attempts);
    }

    [Fact]
    public void RenderKeepsSingleHeadingSpanishTextsAndAccessibleIcons()
    {
        using var context = new BunitContext();
        context.Services.AddSingleton<IPropertyCatalogService>(new StubCatalogService
        {
            CatalogHandler = (_, _) => Task.FromResult(CatalogTestData.Catalog(CatalogTestData.Card()))
        });
        context.Services.AddSingleton(new InteractionContext());

        var cut = context.Render<Home>();

        cut.WaitForAssertion(() => Assert.Contains("Piso luminoso", cut.Markup));
        Assert.Single(cut.FindAll("h1"));
        Assert.Contains("aria-labelledby=\"catalogo-titulo\"", cut.Markup);
        Assert.Contains("aria-hidden=\"true\"", cut.Markup);
        Assert.Contains("Catálogo de propiedades", cut.Markup);
        Assert.DoesNotContain("Available", cut.Markup);
        Assert.All(cut.FindAll("a[href]"), link => Assert.False(string.IsNullOrWhiteSpace(link.GetAttribute("href"))));
    }
}
