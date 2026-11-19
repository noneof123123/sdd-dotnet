using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Http.Results;

namespace RealtorApi.Infrastructure.Results;

public static class ProblemDetailsMapper
{
    public static IResult ToProblemDetails(Result result)
    {
        if (result.IsSuccess || result.Error is null)
            return Microsoft.AspNetCore.Http.Results.Ok();

        return result.Error.Type switch
        {
            ErrorType.Validation => Microsoft.AspNetCore.Http.Results.BadRequest(new ValidationProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = result.Error.Title,
                Detail = result.Error.Detail,
                Errors = new Dictionary<string, string[]>
                {
                    [result.Error.Code] = [result.Error.Detail]
                }
            }),
            ErrorType.NotFound => Microsoft.AspNetCore.Http.Results.NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = result.Error.Title,
                Detail = result.Error.Detail
            }),
            ErrorType.Conflict => Microsoft.AspNetCore.Http.Results.Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = result.Error.Title,
                Detail = result.Error.Detail
            }),
            _ => Microsoft.AspNetCore.Http.Results.Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Error interno")
        };
    }
}
