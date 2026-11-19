using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RealtorApi.Features.Properties.ListProperties;
using RealtorApi.Infrastructure.Endpoints;
using RealtorApi.Infrastructure.Handlers;
using RealtorApi.Infrastructure.Persistence;

namespace RealtorApiTests.Features.Properties.ListProperties;

public sealed class ListPropertiesSliceTests
{
    [Fact]
    public void Slice_IsPublicConcreteAndDiscoverable()
    {
        Assert.True(typeof(ISlice).IsAssignableFrom(typeof(ListPropertiesSlice)));
        Assert.False(typeof(ListPropertiesSlice).IsAbstract);
    }

    [Fact]
    public void Handler_IsRegisteredByAssemblyScanning()
    {
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.RegisterHandlers(typeof(ListPropertiesHandler).Assembly);

        using var provider = services.BuildServiceProvider();
        var handlers = provider.GetServices<IHandler>();

        Assert.Contains(handlers, handler => handler is ListPropertiesHandler);
    }
}
