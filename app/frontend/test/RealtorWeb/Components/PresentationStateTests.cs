using Bunit;
using PropertyState = RealtorWeb.Features.Properties.PresentationState;
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

        var cut = context.Render<PresentationStateComponent>(parameters =>
        {
            parameters.Add(item => item.State, PropertyState.Cargando);
            parameters.Add(item => item.Message, PropertyPresentation.LoadingCatalogMessage);
            parameters.Add(item => item.RetryUrl, "/?page=2");
        });

        Assert.DoesNotContain("href=", cut.Markup);
    }
}
