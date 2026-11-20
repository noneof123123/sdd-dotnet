using Microsoft.AspNetCore.Http;
using RealtorApi.Domain.Properties;
using RealtorApi.Features.Properties.ChangePropertyStatus;
using RealtorApi.Features.Properties.GetPropertyById;

namespace RealtorApiTests.Features.Properties.ChangePropertyStatus;

public sealed class ChangePropertyStatusMappingTests
{
    private static readonly Guid PropertyId = Guid.Parse("3f8b1c2d-4e5a-4b6c-8d9e-0a1b2c3d4e5f");

    [Theory]
    [InlineData("http", "localhost:5023")]
    [InlineData("https", "api.example.test")]
    public void ToResponse_ProjectsAllFieldsWithAbsoluteImageUrl(string scheme, string host)
    {
        var property = CreateProperty("/images/photo.png");
        var request = CreateRequest(scheme, host);

        var response = ChangePropertyStatusMapping.ToResponse(property, request);

        Assert.Equal(property.Id, response.Id);
        Assert.Equal(property.Title, response.Title);
        Assert.Equal(property.Description, response.Description);
        Assert.Equal(property.Address, response.Address);
        Assert.Equal(property.Price, response.Price);
        Assert.Equal(property.Status.ToString(), response.Status);
        Assert.Equal(property.BedroomCount, response.BedroomCount);
        Assert.Equal(property.BathroomCount, response.BathroomCount);
        Assert.Equal(property.AreaSquareMeters, response.AreaSquareMeters);
        Assert.Equal($"{scheme}://{host}/assets/properties/photo.png", response.ImageUrl);
        Assert.Equal(DateTimeKind.Utc, response.CreatedAt.Kind);
        Assert.Equal(DateTimeKind.Utc, response.UpdatedAt!.Value.Kind);
        Assert.DoesNotContain("images", response.ImageUrl, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("..")]
    [InlineData("/assets/properties/")]
    public void ToResponse_WithInvalidOrMissingImage_ReturnsEmptyImageUrl(string? persistedImageUrl)
    {
        var property = CreateProperty(persistedImageUrl);

        var response = ChangePropertyStatusMapping.ToResponse(
            property,
            CreateRequest("http", "localhost:5023"));

        Assert.Equal(string.Empty, response.ImageUrl);
    }

    [Fact]
    public void ToResponse_NormalizesTimestampsToUtc()
    {
        var property = CreateProperty(null);
        property.CreatedAt = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Local);
        property.UpdatedAt = new DateTime(2026, 2, 1, 12, 0, 0, DateTimeKind.Unspecified);

        var response = ChangePropertyStatusMapping.ToResponse(
            property,
            CreateRequest("http", "localhost:5023"));

        Assert.Equal(DateTimeKind.Utc, response.CreatedAt.Kind);
        Assert.Equal(property.CreatedAt.ToUniversalTime(), response.CreatedAt);
        Assert.Equal(DateTimeKind.Utc, response.UpdatedAt!.Value.Kind);
        Assert.Equal(
            DateTime.SpecifyKind(property.UpdatedAt.Value, DateTimeKind.Utc),
            response.UpdatedAt.Value);
    }

    [Fact]
    public void ToResponse_PreservesNullUpdatedAt()
    {
        var property = CreateProperty(null);
        property.UpdatedAt = null;

        var response = ChangePropertyStatusMapping.ToResponse(
            property,
            CreateRequest("http", "localhost:5023"));

        Assert.Null(response.UpdatedAt);
    }

    [Theory]
    [InlineData(PropertyStatus.Available)]
    [InlineData(PropertyStatus.Rented)]
    [InlineData(PropertyStatus.Maintenance)]
    public void ToResponse_MatchesGetPropertyByIdRepresentation(PropertyStatus status)
    {
        var property = CreateProperty("/images/photo.png");
        property.Status = status;
        var request = CreateRequest("https", "api.example.test");

        var changed = ChangePropertyStatusMapping.ToResponse(property, request);
        var queried = GetPropertyByIdMapping.ToResponse(property, request);

        Assert.Equal(queried.Id, changed.Id);
        Assert.Equal(queried.Title, changed.Title);
        Assert.Equal(queried.Description, changed.Description);
        Assert.Equal(queried.Address, changed.Address);
        Assert.Equal(queried.Price, changed.Price);
        Assert.Equal(queried.Status, changed.Status);
        Assert.Equal(queried.BedroomCount, changed.BedroomCount);
        Assert.Equal(queried.BathroomCount, changed.BathroomCount);
        Assert.Equal(queried.AreaSquareMeters, changed.AreaSquareMeters);
        Assert.Equal(queried.ImageUrl, changed.ImageUrl);
        Assert.Equal(queried.CreatedAt, changed.CreatedAt);
        Assert.Equal(queried.UpdatedAt, changed.UpdatedAt);
    }

    private static Property CreateProperty(string? imageUrl) => new()
    {
        Id = PropertyId,
        Title = "Casa con fotografía",
        Description = "Propiedad publicada con PNG válido.",
        Address = "Avenida Central 456",
        Price = 320000m,
        Status = PropertyStatus.Available,
        BedroomCount = 4,
        BathroomCount = 3,
        AreaSquareMeters = 180m,
        ImageUrl = imageUrl,
        CreatedAt = new DateTime(2026, 11, 20, 10, 0, 0, DateTimeKind.Utc),
        UpdatedAt = new DateTime(2026, 11, 20, 11, 45, 0, DateTimeKind.Utc)
    };

    private static HttpRequest CreateRequest(string scheme, string host)
    {
        var context = new DefaultHttpContext();
        context.Request.Scheme = scheme;
        context.Request.Host = new HostString(host);
        return context.Request;
    }
}
