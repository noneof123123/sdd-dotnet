using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace RealtorApiTests.Infrastructure.Validation;

public sealed class ValidationProblemDetailsTests
{
    [Fact]
    public async Task ValidationFailure_ReturnsValidationProblemDetailsWith400()
    {
        var errors = new Dictionary<string, string[]>
        {
            ["Name"] = ["El nombre es obligatorio"]
        };

        var result = Microsoft.AspNetCore.Http.Results.ValidationProblem(
            errors,
            statusCode: StatusCodes.Status400BadRequest);

        var services = new ServiceCollection();
        services.AddLogging();
        using var provider = services.BuildServiceProvider();

        var context = new DefaultHttpContext
        {
            RequestServices = provider
        };
        context.Response.Body = new MemoryStream();

        await result.ExecuteAsync(context);

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.Equal("application/problem+json", context.Response.ContentType?.Split(';')[0]);
    }
}
