using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using RealtorApi.Infrastructure.Endpoints;

namespace RealtorApiTests.Infrastructure.Endpoints;

public sealed class TestSlice : ISlice
{
    public void AddEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/test-slice", () => Microsoft.AspNetCore.Http.Results.Ok());
    }
}
