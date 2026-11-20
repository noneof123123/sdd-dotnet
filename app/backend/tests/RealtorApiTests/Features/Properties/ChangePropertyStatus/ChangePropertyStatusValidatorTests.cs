using RealtorApi.Features.Properties.ChangePropertyStatus;

namespace RealtorApiTests.Features.Properties.ChangePropertyStatus;

public sealed class ChangePropertyStatusValidatorTests
{
    [Theory]
    [InlineData("Available")]
    [InlineData("Rented")]
    [InlineData("Maintenance")]
    public async Task Validate_WithExactAllowedStatus_ReturnsValid(string status)
    {
        var validator = new ChangePropertyStatusValidator();

        var result = await validator.ValidateAsync(CreateRequest(status));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("no-es-un-guid")]
    public async Task Validate_WithMissingOrMalformedId_ReturnsIdError(string? id)
    {
        var validator = new ChangePropertyStatusValidator();
        var request = new ChangePropertyStatusRequest
        {
            Id = id,
            Body = new ChangePropertyStatusBody { Status = "Rented" }
        };

        var result = await validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(ChangePropertyStatusRequest.Id));
    }

    [Fact]
    public async Task Validate_WithoutBody_ReturnsStatusError()
    {
        var validator = new ChangePropertyStatusValidator();
        var request = new ChangePropertyStatusRequest
        {
            Id = Guid.NewGuid().ToString(),
            Body = null
        };

        var result = await validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error =>
            error.PropertyName == nameof(ChangePropertyStatusBody.Status));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Unknown")]
    [InlineData("rented")]
    [InlineData("RENTED")]
    [InlineData("1")]
    public async Task Validate_WithMissingOrInvalidStatus_ReturnsStatusError(string? status)
    {
        var validator = new ChangePropertyStatusValidator();

        var result = await validator.ValidateAsync(CreateRequest(status));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error =>
            error.PropertyName == nameof(ChangePropertyStatusBody.Status));
    }

    private static ChangePropertyStatusRequest CreateRequest(string? status) => new()
    {
        Id = Guid.NewGuid().ToString(),
        Body = new ChangePropertyStatusBody { Status = status }
    };
}
