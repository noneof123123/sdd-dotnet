using RealtorApi.Infrastructure.Endpoints;
using RealtorApi.Infrastructure.OpenApi;

namespace RealtorApi.Features.Health;

public sealed class HealthSlice : ISlice
{
    public void AddEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/health", () => Results.Ok(new HealthResponse("healthy")))
            .WithName(RealtorOpenApiConstants.GetHealthOperationId)
            .WithTags(RealtorOpenApiConstants.HealthTag)
            .Produces<HealthResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}
