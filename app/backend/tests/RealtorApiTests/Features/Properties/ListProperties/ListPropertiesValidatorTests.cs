using RealtorApi.Features.Properties.ListProperties;

namespace RealtorApiTests.Features.Properties.ListProperties;

public sealed class ListPropertiesValidatorTests
{
    [Theory]
    [InlineData(0, 6)]
    [InlineData(-1, 6)]
    [InlineData(1, 0)]
    [InlineData(1, -1)]
    public void ValidateAsync_ReturnsValidationErrorsForInvalidParameters(int page, int pageSize)
    {
        var validator = new ListPropertiesValidator();

        var result = validator.Validate(new ListPropertiesRequest
        {
            Page = page,
            PageSize = pageSize
        });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void ValidParameters_AreAccepted()
    {
        var validator = new ListPropertiesValidator();

        var result = validator.Validate(new ListPropertiesRequest
        {
            Page = 1,
            PageSize = 24
        });

        Assert.True(result.IsValid);
    }
}
