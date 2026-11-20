using RealtorApi.Domain.Properties;

namespace RealtorApi.Features.Properties.UpdateProperty;

public static class UpdatePropertyMapping
{
    public static UpdatePropertyResponse ToResponse(Property property) => new(
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
