using RealtorApi.Infrastructure.Endpoints;
using RealtorApi.Infrastructure.Handlers;

namespace RealtorApi.Features.Properties.ListProperties;

public sealed class ListPropertiesSlice : ISlice
{
    public void AddEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/properties", async (
            [AsParameters] ListPropertiesRequest request,
            HttpContext httpContext,
            ListPropertiesHandler handler,
            CancellationToken cancellationToken) =>
        {
            var normalizedRequest = new ListPropertiesRequest
            {
                Page = request.Page,
                PageSize = request.PageSize
            };

            var result = await handler.HandleAsync(normalizedRequest, httpContext.Request, cancellationToken);
            return Results.Ok(result);
        });
    }
}
