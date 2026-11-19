using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using RealtorApi.Infrastructure.Results;

namespace RealtorApiTests.Infrastructure.Results;

public sealed class ProblemDetailsMapperTests
{
    [Theory]
    [InlineData(ErrorType.Validation, StatusCodes.Status400BadRequest)]
    [InlineData(ErrorType.NotFound, StatusCodes.Status404NotFound)]
    [InlineData(ErrorType.Conflict, StatusCodes.Status409Conflict)]
    public void ResultError_MapsToExpectedStatus(ErrorType type, int expectedStatus)
    {
        var error = type switch
        {
            ErrorType.Validation => Error.Validation("validation", "Detalle seguro"),
            ErrorType.NotFound => Error.NotFound("not_found", "Detalle seguro"),
            ErrorType.Conflict => Error.Conflict("conflict", "Detalle seguro"),
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };

        var response = ProblemDetailsMapper.ToProblemDetails(Result.Failure(error));

        Assert.Equal(expectedStatus, GetStatusCode(response));
    }

    private static int GetStatusCode(IResult result)
    {
        return result switch
        {
            BadRequest<Microsoft.AspNetCore.Mvc.ValidationProblemDetails> response => response.StatusCode,
            NotFound<Microsoft.AspNetCore.Mvc.ProblemDetails> response => response.StatusCode,
            Conflict<Microsoft.AspNetCore.Mvc.ProblemDetails> response => response.StatusCode,
            _ => throw new InvalidOperationException(result.GetType().Name)
        };
    }
}
