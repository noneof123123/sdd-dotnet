namespace RealtorApi.Features.Properties.ListProperties;

public sealed record ListPropertiesItemResponse(
    Guid Id,
    string Title,
    string Description,
    string Address,
    decimal Price,
    string Status,
    int BedroomCount,
    int BathroomCount,
    decimal AreaSquareMeters,
    string ImageUrl);

public sealed record ListPropertiesResponse(
    IReadOnlyList<ListPropertiesItemResponse> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages,
    bool HasNext,
    bool HasPrevious);
