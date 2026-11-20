using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using RealtorApi.Infrastructure.Endpoints;
using RealtorApi.Infrastructure.OpenApi;
using RealtorApi.Infrastructure.Results;

namespace RealtorApi.Features.Properties.GetPropertyById;

public sealed class GetPropertyByIdSlice : ISlice
{
    public void AddEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/properties/{id}", async (
            [AsParameters] GetPropertyByIdRequest request,
            HttpContext httpContext,
            GetPropertyByIdHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(request, httpContext.Request, cancellationToken);

            return result.IsSuccess && result.Value is not null
                ? Results.Ok(result.Value)
                : ProblemDetailsMapper.ToProblemDetails(result);
        })
        .WithName(RealtorOpenApiConstants.GetPropertyByIdOperationId)
        .WithTags(RealtorOpenApiConstants.PropertiesTag)
        .Produces<GetPropertyByIdResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
