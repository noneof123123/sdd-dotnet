using FluentValidation;

namespace RealtorApiTests.Infrastructure.Validation;

public sealed record TestRequest(string Name);

public sealed class TestRequestValidator : AbstractValidator<TestRequest>
{
    public TestRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty();
    }
}

public sealed record RequestWithoutValidator(string Name);
