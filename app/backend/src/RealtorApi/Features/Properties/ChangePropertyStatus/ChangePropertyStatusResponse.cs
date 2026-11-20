namespace RealtorApi.Features.Properties.ChangePropertyStatus;

public sealed record ChangePropertyStatusResponse(
    Guid Id,
    string Title,
    string Description,
    string Address,
    decimal Price,
    string Status,
    int BedroomCount,
    int BathroomCount,
    decimal AreaSquareMeters,
    string ImageUrl,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
