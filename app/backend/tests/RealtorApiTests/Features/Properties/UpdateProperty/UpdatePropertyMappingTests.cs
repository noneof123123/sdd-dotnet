using RealtorApi.Domain.Properties;
using RealtorApi.Features.Properties.UpdateProperty;

namespace RealtorApiTests.Features.Properties.UpdateProperty;

public sealed class UpdatePropertyMappingTests
{
    [Fact]
    public void ToResponse_ProjectsAllPublicFieldsWithoutEntityOrTimestamps()
    {
        var id = Guid.NewGuid();
        var property = new Property
        {
            Id = id,
            Title = "Casa actualizada",
            Description = "Descripción vigente.",
            Address = "Calle Central 123",
            Price = 275000.00m,
            Status = PropertyStatus.Rented,
            BedroomCount = 3,
            BathroomCount = 2,
            AreaSquareMeters = 145.50m,
            ImageUrl = "/assets/properties/generated.png",
            CreatedAt = new DateTime(2026, 11, 20, 12, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2026, 11, 21, 12, 0, 0, DateTimeKind.Utc)
        };

        var response = UpdatePropertyMapping.ToResponse(property);

        var typed = Assert.IsType<UpdatePropertyResponse>(response);
        Assert.NotSame(property, response);
        Assert.Equal(id, typed.Id);
        Assert.Equal("Casa actualizada", typed.Title);
        Assert.Equal("Descripción vigente.", typed.Description);
        Assert.Equal("Calle Central 123", typed.Address);
        Assert.Equal(275000.00m, typed.Price);
        Assert.Equal("Rented", typed.Status);
        Assert.Equal(3, typed.BedroomCount);
        Assert.Equal(2, typed.BathroomCount);
        Assert.Equal(145.50m, typed.AreaSquareMeters);
        Assert.Equal("/assets/properties/generated.png", typed.ImageUrl);
        Assert.DoesNotContain(typed.GetType().GetProperties(), propertyInfo => propertyInfo.Name.Contains("Created", StringComparison.Ordinal));
        Assert.DoesNotContain(typed.GetType().GetProperties(), propertyInfo => propertyInfo.Name.Contains("Updated", StringComparison.Ordinal));
    }

    [Fact]
    public void ToResponse_PreservesNullRelativeImageUrl()
    {
        var property = new Property
        {
            Id = Guid.NewGuid(),
            Title = "Casa sin imagen",
            Description = "Propiedad sin fotografía.",
            Address = "Avenida Central 456",
            Price = 180000m,
            Status = PropertyStatus.Available,
            BedroomCount = 2,
            BathroomCount = 1,
            AreaSquareMeters = 95m,
            ImageUrl = null
        };

        var response = UpdatePropertyMapping.ToResponse(property);

        Assert.Null(response.ImageUrl);
    }
}
