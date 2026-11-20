using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using RealtorApi.Infrastructure.Endpoints;
using RealtorApi.Infrastructure.Results;

namespace RealtorApi.Features.Properties.UpdateProperty;

public sealed class UpdatePropertySlice : ISlice
{
    public void AddEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/properties/{id}", async (
            [AsParameters] UpdatePropertyRequest request,
            UpdatePropertyHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(request, cancellationToken);

            return result.IsSuccess && result.Value is not null
                ? Results.Ok(result.Value)
                : ProblemDetailsMapper.ToProblemDetails(result);
        });
    }
}
