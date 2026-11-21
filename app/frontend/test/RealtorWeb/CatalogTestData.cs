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
        CatalogWith(page: 2, totalItems: items.Length * 2, totalPages: 3, items: items);

    public static PropertyCatalogViewModel CatalogWith(
        int page = 2,
        int totalItems = 2,
        int totalPages = 3,
        bool hasPrevious = true,
        bool hasNext = true,
        PresentationState state = PresentationState.Contenido,
        string? message = null,
        string? retryUrl = null,
        params PropertyCardViewModel[] items) =>
        new(
            items,
            page,
            CatalogNavigation.PageSize,
            totalItems,
            totalPages,
            hasNext,
            hasPrevious,
            CatalogNavigation.PreviousUrl(page, hasPrevious),
            CatalogNavigation.NextUrl(page, hasNext),
            $"Página {page} de {totalPages}",
            state,
            message ?? PropertyPresentation.MessageFor(state),
            retryUrl);

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
