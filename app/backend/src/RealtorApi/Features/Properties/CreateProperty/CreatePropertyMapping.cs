using RealtorApi.Domain.Properties;
using RealtorApi.Features.Properties.CreateProperty;

namespace RealtorApi.Features.Properties.CreateProperty;

public static class CreatePropertyMapping
{
    public static CreatePropertyResponse ToResponse(Property property) => new(
        property.Id,
        property.Title,
        property.Description,
        property.Address,
        property.Price,
        property.Status.ToString(),
        property.BedroomCount,
        property.BathroomCount,
        property.AreaSquareMeters,
        property.ImageUrl);
}
