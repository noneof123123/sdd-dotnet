using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using RealtorApi.Infrastructure.Endpoints;
using RealtorApi.Infrastructure.Results;

namespace RealtorApi.Features.Properties.CreateProperty;

public sealed class CreatePropertySlice : ISlice
{
    public void AddEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/properties", async (
            [AsParameters] CreatePropertyRequest request,
            CreatePropertyHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(request, cancellationToken);

            return result.IsSuccess && result.Value is not null
                ? Results.Created($"/api/properties/{result.Value.Id}", result.Value)
                : ProblemDetailsMapper.ToProblemDetails(result);
        });
    }
}
