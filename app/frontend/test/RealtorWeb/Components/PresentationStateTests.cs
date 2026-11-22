using Bunit;
using Microsoft.Extensions.DependencyInjection;
using PropertyState = RealtorWeb.Features.Properties.PresentationState;
using RealtorWeb.Components.Pages;
using RealtorWeb.Features.Interactions;
using RealtorWeb.Features.Properties;
using PresentationStateComponent = RealtorWeb.Components.Properties.PresentationState;

namespace RealtorWebTests.Components;

public class PresentationStateTests
{
    [Theory]
    [InlineData(PropertyState.Cargando, PropertyPresentation.LoadingCatalogMessage, "status")]
    [InlineData(PropertyState.Vacio, PropertyPresentation.EmptyMessage, "status")]
    [InlineData(PropertyState.Error, PropertyPresentation.ErrorMessage, "alert")]
    [InlineData(PropertyState.NoEncontrado, PropertyPresentation.NotFoundMessage, "status")]
    public void RenderShowsMessageAndAccessibleRole(PropertyState state, string message, string expectedRole)
    {
        using var context = new BunitContext();
        context.Services.AddSingleton(new InteractionContext());

        var cut = context.Render<PresentationStateComponent>(parameters =>
        {
            parameters.Add(item => item.State, state);
            parameters.Add(item => item.Message, message);
        });

        Assert.Contains(message, cut.Markup);
        Assert.Contains($"role=\"{expectedRole}\"", cut.Markup);
        Assert.Contains("aria-hidden=\"true\"", cut.Markup);
    }

    [Fact]
    public void RenderErrorShowsRetryAction()
    {
        using var context = new BunitContext();
        context.Services.AddSingleton(new InteractionContext());

        var cut = context.Render<PresentationStateComponent>(parameters =>
        {
            parameters.Add(item => item.State, PropertyState.Error);
            parameters.Add(item => item.Message, PropertyPresentation.ErrorMessage);
            parameters.Add(item => item.RetryUrl, "/?page=2");
        });

        Assert.Contains("Reintentar", cut.Markup);
        Assert.Contains("href=\"/?page=2\"", cut.Markup);
    }

    [Fact]
    public void RenderNotFoundShowsBackAction()
    {
        using var context = new BunitContext();
        context.Services.AddSingleton(new InteractionContext());

        var cut = context.Render<PresentationStateComponent>(parameters =>
        {
            parameters.Add(item => item.State, PropertyState.NoEncontrado);
            parameters.Add(item => item.Message, PropertyPresentation.NotFoundMessage);
            parameters.Add(item => item.RetryUrl, "/?page=3");
        });

        Assert.Contains("Volver al catálogo", cut.Markup);
        Assert.Contains("href=\"/?page=3\"", cut.Markup);
    }

    [Fact]
    public void RenderLoadingDoesNotShowAction()
    {
        using var context = new BunitContext();
        context.Services.AddSingleton(new InteractionContext());

        var cut = context.Render<PresentationStateComponent>(parameters =>
        {
            parameters.Add(item => item.State, PropertyState.Cargando);
            parameters.Add(item => item.Message, PropertyPresentation.LoadingCatalogMessage);
            parameters.Add(item => item.RetryUrl, "/?page=2");
        });

        Assert.DoesNotContain("href=", cut.Markup);
    }

    [Fact]
    public void CatalogDoesNotRenderNotFoundState()
    {
        using var context = new BunitContext();
        context.Services.AddSingleton(new InteractionContext());
        context.Services.AddSingleton<IPropertyCatalogService>(
            new FixedCatalogService(CatalogTestData.Catalog()));

        var cut = context.Render<Home>();

        cut.WaitForAssertion(() => Assert.Contains("Página 2 de 3", cut.Markup));
        Assert.DoesNotContain(PropertyPresentation.NotFoundMessage, cut.Markup);
        Assert.DoesNotContain("Volver al catálogo", cut.Markup);
    }

    private sealed class FixedCatalogService(PropertyCatalogViewModel catalog) : IPropertyCatalogService
    {
        public Task<PropertyCatalogViewModel> GetCatalogAsync(int page, CancellationToken cancellationToken) =>
            Task.FromResult(catalog);

        public Task<PropertyDetailsViewModel> GetDetailsAsync(
            Guid id,
            int page,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }
}
