using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using RealtorWeb.Components.Pages;
using RealtorWeb.Features.Interactions;
using RealtorWeb.Features.Properties;

namespace RealtorWebTests.Components;

public class PropertyDetailsStateTests
{
    [Fact]
    public void RenderShowsLoadingBeforeServiceCompletes()
    {
        using var context = new BunitContext();
        context.Services.AddSingleton(new InteractionContext());
        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var completion = new TaskCompletionSource<PropertyDetailsViewModel>(TaskCreationOptions.RunContinuationsAsynchronously);
        var service = new StubCatalogService
        {
            DetailsHandler = (_, _, _) => completion.Task
        };
        context.Services.AddSingleton<IPropertyCatalogService>(service);

        var cut = context.Render<PropertyDetails>(parameters => parameters.Add(item => item.Id, id));

        Assert.Contains(PropertyPresentation.LoadingDetailsMessage, cut.Markup);
        Assert.DoesNotContain("Piso luminoso", cut.Markup);

        completion.SetResult(CatalogTestData.Details(id: id, page: 1));
        cut.WaitForAssertion(() => Assert.Contains("Piso luminoso", cut.Markup));
    }

    [Fact]
    public void RenderNotFoundDoesNotShowPartialPropertyData()
    {
        using var context = new BunitContext();
        context.Services.AddSingleton(new InteractionContext());
        var id = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var service = new StubCatalogService
        {
            DetailsHandler = (_, page, _) =>
                Task.FromResult(CatalogTestData.Details(
                    id: id,
                    page: page,
                    state: PresentationState.NoEncontrado))
        };
        context.Services.AddSingleton<IPropertyCatalogService>(service);

        var cut = context.Render<PropertyDetails>(parameters => parameters.Add(item => item.Id, id));

        cut.WaitForAssertion(() => Assert.Contains(PropertyPresentation.NotFoundMessage, cut.Markup));
        Assert.Contains("Volver al catálogo", cut.Markup);
        Assert.DoesNotContain("Piso luminoso", cut.Markup);
        Assert.DoesNotContain("1.250,00", cut.Markup);
        Assert.DoesNotContain("Descripción completa", cut.Markup);
    }

    [Fact]
    public void RenderErrorShowsRetryForSameDetail()
    {
        using var context = new BunitContext();
        context.Services.AddSingleton(new InteractionContext());
        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var service = new StubCatalogService
        {
            DetailsHandler = (requestedId, page, _) =>
                Task.FromResult(CatalogTestData.Details(
                    id: requestedId,
                    page: page,
                    state: PresentationState.Error,
                    retryUrl: CatalogNavigation.RetryDetailsUrl(requestedId, page)))
        };
        context.Services.AddSingleton<IPropertyCatalogService>(service);
        var navigation = context.Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo(navigation.GetUriWithQueryParameter("page", "2"));

        var cut = context.Render<PropertyDetails>(parameters => parameters.Add(item => item.Id, id));

        cut.WaitForAssertion(() => Assert.Contains(PropertyPresentation.ErrorMessage, cut.Markup));
        Assert.Contains("Reintentar", cut.Markup);
        Assert.Contains("<button type=\"button\"", cut.Markup);
        Assert.DoesNotContain("Piso luminoso", cut.Markup);
    }
}
