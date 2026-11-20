using Microsoft.AspNetCore.Http;
using RealtorApi.Domain.Properties;
using RealtorApi.Features.Properties.GetPropertyById;
using RealtorApi.Features.Properties.ListProperties;

namespace RealtorApiTests.Features.Properties.GetPropertyById;

public sealed class GetPropertyByIdMappingTests
{
    private static readonly Guid PropertyId = Guid.Parse("3f8b1c2d-4e5a-4b6c-8d9e-0a1b2c3d4e5f");

    [Theory]
    [InlineData("http", "localhost:5023")]
    [InlineData("https", "api.example.test")]
    public void ToResponse_ProjectsAllFieldsWithAbsoluteImageUrl(string scheme, string host)
    {
        var property = CreateProperty("/images/photo.png");
        var request = CreateRequest(scheme, host);

        var response = GetPropertyByIdMapping.ToResponse(property, request);

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

        var response = GetPropertyByIdMapping.ToResponse(property, CreateRequest("http", "localhost:5023"));

        Assert.Equal(string.Empty, response.ImageUrl);
    }

    [Fact]
    public void ToResponse_NormalizesTimestampsToUtc()
    {
        var property = CreateProperty(null);
        property.CreatedAt = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Local);
        property.UpdatedAt = new DateTime(2026, 2, 1, 12, 0, 0, DateTimeKind.Unspecified);

        var response = GetPropertyByIdMapping.ToResponse(property, CreateRequest("http", "localhost:5023"));

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

        var response = GetPropertyByIdMapping.ToResponse(property, CreateRequest("http", "localhost:5023"));

        Assert.Null(response.UpdatedAt);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("..")]
    [InlineData("/images/photo.png")]
    [InlineData("photo.jpg")]
    public void ToResponse_MatchesListMappingForSharedFields(string? persistedImageUrl)
    {
        var property = CreateProperty(persistedImageUrl);
        var request = CreateRequest("https", "api.example.test");

        var detail = GetPropertyByIdMapping.ToResponse(property, request);
        var list = ListPropertiesMapping.ToResponse(
            property.Id,
            property.Title,
            property.Description,
            property.Address,
            property.Price,
            property.Status.ToString(),
            property.BedroomCount,
            property.BathroomCount,
            property.AreaSquareMeters,
            property.ImageUrl,
            request);

        Assert.Equal(list.Id, detail.Id);
        Assert.Equal(list.Title, detail.Title);
        Assert.Equal(list.Description, detail.Description);
        Assert.Equal(list.Address, detail.Address);
        Assert.Equal(list.Price, detail.Price);
        Assert.Equal(list.Status, detail.Status);
        Assert.Equal(list.BedroomCount, detail.BedroomCount);
        Assert.Equal(list.BathroomCount, detail.BathroomCount);
        Assert.Equal(list.AreaSquareMeters, detail.AreaSquareMeters);
        Assert.Equal(list.ImageUrl, detail.ImageUrl);
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
