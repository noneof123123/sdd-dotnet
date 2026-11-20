using FluentValidation;
using RealtorApi.Features.Properties.GetPropertyById;

namespace RealtorApiTests.Features.Properties.GetPropertyById;

public sealed class GetPropertyByIdValidatorTests
{
    [Fact]
    public async Task Validate_WithValidId_ReturnsValid()
    {
        var validator = new GetPropertyByIdValidator();
        var request = new GetPropertyByIdRequest { Id = Guid.NewGuid() };

        var result = await validator.ValidateAsync(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithEmptyId_ReturnsError()
    {
        var validator = new GetPropertyByIdValidator();
        var request = new GetPropertyByIdRequest { Id = Guid.Empty };

        var result = await validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(GetPropertyByIdRequest.Id));
    }

    [Fact]
    public void Validator_IsRegisteredAsFluentValidator()
    {
        Assert.True(typeof(IValidator<GetPropertyByIdRequest>).IsAssignableFrom(typeof(GetPropertyByIdValidator)));
    }
}
