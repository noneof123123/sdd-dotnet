using Bunit;
using RealtorWeb.Components.Layout;

namespace RealtorWebTests.Components.Layout;

public class ReconnectModalTests
{
    [Fact]
    public void RenderUsesFrameworkIdentifiersClassesAndSpanishTexts()
    {
        using var context = new BunitContext();

        var cut = context.Render<ReconnectModal>();

        Assert.Contains("id=\"components-reconnect-modal\"", cut.Markup);
        Assert.Contains("components-reconnect-show", cut.Markup);
        Assert.Contains("components-reconnect-failed", cut.Markup);
        Assert.Contains("components-reconnect-rejected", cut.Markup);
        Assert.Contains("Reconectando…", cut.Markup);
        Assert.Contains("No se pudo mantener la conexión.", cut.Markup);
        Assert.Contains("Reintentar", cut.Markup);
        Assert.Contains("Recargar página", cut.Markup);
    }

    [Fact]
    public void RenderHasAccessibleButtonsWithoutInlineStyles()
    {
        using var context = new BunitContext();

        var cut = context.Render<ReconnectModal>();

        Assert.All(cut.FindAll("button"), button => Assert.Equal("button", button.GetAttribute("type")));
        Assert.Single(cut.FindAll("[data-reconnect-action='retry']"));
        Assert.Equal(2, cut.FindAll("[data-reconnect-action='reload']").Count);
        Assert.Contains("aria-labelledby=\"reconnect-titulo\"", cut.Markup);
        Assert.Contains("aria-describedby=\"reconnect-mensaje\"", cut.Markup);
        Assert.Contains("aria-hidden=\"true\"", cut.Markup);
        Assert.DoesNotContain("style=", cut.Markup);
    }

    [Fact]
    public void RenderHasComprehensibleSpanishLabelsAndNoTechnicalDetails()
    {
        using var context = new BunitContext();

        var cut = context.Render<ReconnectModal>();

        Assert.Single(cut.FindAll("#components-reconnect-modal"));
        Assert.Equal(3, cut.FindAll("[role='alertdialog']").Count);
        Assert.All(cut.FindAll("button"), button =>
            Assert.Contains(button.TextContent, new[] { "Reintentar", "Recargar página" }));
        Assert.DoesNotContain("SignalR", cut.Markup);
        Assert.DoesNotContain("WebSocket", cut.Markup);
        Assert.DoesNotContain("circuito", cut.Markup);
    }
}
