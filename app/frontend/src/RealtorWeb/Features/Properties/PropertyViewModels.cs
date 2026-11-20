namespace RealtorWeb.Features.Properties;

public enum PresentationState
{
    Cargando,
    Contenido,
    Vacio,
    Error,
    NoEncontrado
}

public sealed record PropertyCardViewModel(
    Guid Id,
    string Title,
    string Summary,
    string Address,
    string Price,
    string StatusLabel,
    int BedroomCount,
    int BathroomCount,
    int AreaSquareMeters,
    string? ImageUrl,
    string ImageAlt,
    string DetailsUrl);

public sealed record PropertyCatalogViewModel(
    IReadOnlyList<PropertyCardViewModel> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages,
    bool HasNext,
    bool HasPrevious,
    string? PreviousUrl,
    string? NextUrl,
    string SummaryText,
    PresentationState State,
    string Message,
    string? RetryUrl);

public sealed record PropertyDetailsViewModel(
    Guid Id,
    string Title,
    string Description,
    string Address,
    string Price,
    string StatusLabel,
    int BedroomCount,
    int BathroomCount,
    int AreaSquareMeters,
    string? ImageUrl,
    string ImageAlt,
    string CreatedAt,
    string UpdatedAt,
    string BackUrl,
    PresentationState State,
    string Message,
    string? RetryUrl);
