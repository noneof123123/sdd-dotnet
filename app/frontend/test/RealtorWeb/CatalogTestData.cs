using RealtorWeb.Features.Properties;

namespace RealtorWebTests;

public static class CatalogTestData
{
    public static PropertyCardViewModel Card(
        Guid? id = null,
        string title = "Piso luminoso",
        string status = PropertyStatus.Available,
        string? imageUrl = "/images/piso.jpg",
        int page = 1) =>
        new(
            id ?? Guid.Parse("11111111-1111-1111-1111-111111111111"),
            title,
            "Descripción extensa del inmueble.",
            "Calle Mayor 1, Madrid",
            "1.250,00",
            PropertyStatusLabels.Resolve(status),
            3,
            2,
            95,
            imageUrl,
            imageUrl is null ? $"Imagen no disponible para {title}" : $"Imagen de {title}",
            CatalogNavigation.DetailsUrl(id ?? Guid.Parse("11111111-1111-1111-1111-111111111111"), page));

    public static PropertyCatalogViewModel Catalog(params PropertyCardViewModel[] items) =>
        new(
            items,
            2,
            CatalogNavigation.PageSize,
            items.Length * 2,
            3,
            true,
            true,
            "/?page=1",
            "/?page=3",
            "Página 2 de 3",
            PresentationState.Contenido,
            string.Empty,
            null);

    public static PropertyDetailsViewModel Details(
        Guid? id = null,
        string title = "Piso luminoso",
        string status = PropertyStatus.Available,
        string? imageUrl = "/images/piso.jpg",
        int page = 2,
        PresentationState state = PresentationState.Contenido,
        string? retryUrl = null) =>
        new(
            id ?? Guid.Parse("11111111-1111-1111-1111-111111111111"),
            title,
            "Descripción completa del inmueble.",
            "Calle Mayor 1, Madrid",
            "1.250,00",
            PropertyStatusLabels.Resolve(status),
            3,
            2,
            95,
            imageUrl,
            imageUrl is null ? $"Imagen no disponible para {title}" : $"Imagen de {title}",
            "14/05/2026 10:30",
            "20/05/2026 12:45",
            CatalogNavigation.BackUrl(page),
            state,
            PropertyPresentation.MessageFor(state, true),
            retryUrl);
}

public sealed class StubCatalogService : IPropertyCatalogService
{
    public Func<int, CancellationToken, Task<PropertyCatalogViewModel>> CatalogHandler { get; set; } =
        (page, _) => Task.FromResult(CatalogTestData.Catalog());

    public Func<Guid, int, CancellationToken, Task<PropertyDetailsViewModel>> DetailsHandler { get; set; } =
        (id, page, _) => Task.FromResult(CatalogTestData.Details(id: id, page: page));

    public Task<PropertyCatalogViewModel> GetCatalogAsync(int page, CancellationToken cancellationToken) =>
        CatalogHandler(page, cancellationToken);

    public Task<PropertyDetailsViewModel> GetDetailsAsync(Guid id, int page, CancellationToken cancellationToken) =>
        DetailsHandler(id, page, cancellationToken);
}
