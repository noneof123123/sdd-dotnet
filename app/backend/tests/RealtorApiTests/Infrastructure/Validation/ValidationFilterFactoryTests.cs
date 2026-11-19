using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using RealtorApi.Infrastructure.Validation;

namespace RealtorApiTests.Infrastructure.Validation;

public sealed class ValidationFilterFactoryTests
{
    [Fact]
    public async Task RegisteredValidator_IsDetectedAndRejectsInvalidRequest()
    {
        var services = new ServiceCollection();
        services.AddScoped<IValidator<TestRequest>, TestRequestValidator>();
        await using var provider = services.BuildServiceProvider();
        var validator = provider.GetRequiredService<IValidator<TestRequest>>();

        var result = await validator.ValidateAsync(new TestRequest(string.Empty));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(TestRequest.Name));
    }

    [Fact]
    public void MissingValidator_ProducesNoRegisteredValidator()
    {
        var services = new ServiceCollection();
        using var provider = services.BuildServiceProvider();

        Assert.Null(provider.GetService<IValidator<RequestWithoutValidator>>());
    }
}
