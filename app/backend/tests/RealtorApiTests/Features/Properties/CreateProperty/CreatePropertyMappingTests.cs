using RealtorApi.Domain.Properties;
using RealtorApi.Features.Properties.CreateProperty;

namespace RealtorApiTests.Features.Properties.CreateProperty;

public sealed class CreatePropertyMappingTests
{
    [Fact]
    public void ToResponse_ProjectsWithoutImageAndDoesNotExposeEntity()
    {
        var id = Guid.NewGuid();
        var property = new Property
        {
            Id = id,
            Title = "Casa de prueba",
            Description = "Propiedad creada para validación.",
            Address = "Calle Central 123",
            Price = 250000.00m,
            Status = PropertyStatus.Rented,
            BedroomCount = 3,
            BathroomCount = 2,
            AreaSquareMeters = 140.50m,
            ImageUrl = null,
            CreatedAt = new DateTime(2026, 11, 20, 12, 0, 0, DateTimeKind.Utc)
        };

        var response = CreatePropertyMapping.ToResponse(property);

        var typed = Assert.IsType<CreatePropertyResponse>(response);
        Assert.NotSame(property, response);
        Assert.Equal(id, typed.Id);
        Assert.Equal("Casa de prueba", typed.Title);
        Assert.Equal("Propiedad creada para validación.", typed.Description);
        Assert.Equal("Calle Central 123", typed.Address);
        Assert.Equal(250000.00m, typed.Price);
        Assert.Equal("Rented", typed.Status);
        Assert.Equal(3, typed.BedroomCount);
        Assert.Equal(2, typed.BathroomCount);
        Assert.Equal(140.50m, typed.AreaSquareMeters);
        Assert.Null(typed.ImageUrl);
    }

    [Fact]
    public void ToResponse_PreservesRelativeImageUrl()
    {
        var property = new Property
        {
            Id = Guid.NewGuid(),
            Title = "Casa con imagen",
            Description = "Propiedad con fotografía.",
            Address = "Avenida Central 456",
            Price = 320000.00m,
            Status = PropertyStatus.Available,
            BedroomCount = 4,
            BathroomCount = 3,
            AreaSquareMeters = 180m,
            ImageUrl = "/assets/properties/generated.png"
        };

        var response = CreatePropertyMapping.ToResponse(property);

        Assert.Equal("/assets/properties/generated.png", response.ImageUrl);
    }
}
