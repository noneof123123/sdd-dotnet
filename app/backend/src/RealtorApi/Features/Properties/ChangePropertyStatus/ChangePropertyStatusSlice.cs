using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using RealtorApi.Infrastructure.Endpoints;
using RealtorApi.Infrastructure.OpenApi;
using RealtorApi.Infrastructure.Results;

namespace RealtorApi.Features.Properties.ChangePropertyStatus;

public sealed class ChangePropertyStatusSlice : ISlice
{
    public void AddEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("/api/properties/{id}/status", async (
            [FromRoute(Name = "id")] string? id,
            ChangePropertyStatusRequest request,
            HttpContext httpContext,
            ChangePropertyStatusHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(
                request with { Id = id },
                httpContext.Request,
                cancellationToken);

            return result.IsSuccess && result.Value is not null
                ? Results.Ok(result.Value)
                : ProblemDetailsMapper.ToProblemDetails(result);
        })
        .WithName(RealtorOpenApiConstants.ChangePropertyStatusOperationId)
        .WithTags(RealtorOpenApiConstants.PropertiesTag)
        .Accepts<ChangePropertyStatusBody>("application/json")
        .Produces<ChangePropertyStatusResponse>(StatusCodes.Status200OK)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}
