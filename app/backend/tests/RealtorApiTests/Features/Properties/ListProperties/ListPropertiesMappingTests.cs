using Microsoft.AspNetCore.Http;
using RealtorApi.Features.Properties.ListProperties;

namespace RealtorApiTests.Features.Properties.ListProperties;

public sealed class ListPropertiesMappingTests
{
    [Theory]
    [InlineData("http", "localhost:5023")]
    [InlineData("https", "api.example.test")]
    public void ToResponse_BuildsAbsolutePublicImageUrl(string scheme, string host)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = scheme;
        httpContext.Request.Host = new HostString(host);

        var response = ListPropertiesMapping.ToResponse(
            Guid.NewGuid(), "Title", "Description", "Address", 100,
            "Available", 2, 1, 80, "/images/1.png", httpContext.Request);

        Assert.Equal($"{scheme}://{host}/assets/properties/1.png", response.ImageUrl);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("..")]
    public void ToResponse_ReturnsEmptyForInvalidImageName(string imageUrl)
    {
        var httpContext = new DefaultHttpContext();

        var response = ListPropertiesMapping.ToResponse(
            Guid.NewGuid(), "Title", "Description", "Address", 100,
            "Available", 2, 1, 80, imageUrl, httpContext.Request);

        Assert.Equal(string.Empty, response.ImageUrl);
    }
}
