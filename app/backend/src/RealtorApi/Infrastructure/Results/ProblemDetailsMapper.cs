using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Http.Results;

namespace RealtorApi.Infrastructure.Results;

public static class ProblemDetailsMapper
{
    public static IResult ToProblemDetails(Result result)
    {
        if (result.IsSuccess)
            return Microsoft.AspNetCore.Http.Results.Ok();

        return result.Error is null
            ? InternalProblem()
            : ToProblemDetails(result.Error);
    }

    public static IResult ToProblemDetails<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return Microsoft.AspNetCore.Http.Results.Ok();

        return result.Error is null
            ? InternalProblem()
            : ToProblemDetails(result.Error);
    }

    private static IResult ToProblemDetails(Error error) => error.Type switch
    {
        ErrorType.Validation => Microsoft.AspNetCore.Http.Results.BadRequest(new ValidationProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = error.Title,
            Detail = error.Detail,
            Errors = new Dictionary<string, string[]>
            {
                [error.Code] = [error.Detail]
            }
        }),
        ErrorType.NotFound => Microsoft.AspNetCore.Http.Results.NotFound(new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = error.Title,
            Detail = error.Detail
        }),
        ErrorType.Conflict => Microsoft.AspNetCore.Http.Results.Conflict(new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = error.Title,
            Detail = error.Detail
        }),
        ErrorType.Internal => InternalProblem(),
        _ => InternalProblem()
    };

    private static IResult InternalProblem() => Microsoft.AspNetCore.Http.Results.Problem(
        statusCode: StatusCodes.Status500InternalServerError,
        title: "Error interno");
}
