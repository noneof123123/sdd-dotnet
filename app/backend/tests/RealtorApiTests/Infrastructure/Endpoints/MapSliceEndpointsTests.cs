using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using RealtorApi.Infrastructure.Endpoints;

namespace RealtorApiTests.Infrastructure.Endpoints;

public sealed class MapSliceEndpointsTests
{
    [Fact]
    public void MapSliceEndpoints_UsesRegisteredSlices()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.RegisterSlices(typeof(TestSlice).Assembly);

        using var app = builder.Build();
        app.MapSliceEndpoints();

        var slices = app.Services.GetServices<ISlice>().ToArray();
        Assert.Contains(slices, slice => slice is TestSlice);
    }
}
