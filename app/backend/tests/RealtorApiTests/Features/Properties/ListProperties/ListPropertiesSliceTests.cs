using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RealtorApi.Features.Properties.ListProperties;
using RealtorApi.Infrastructure.Endpoints;
using RealtorApi.Infrastructure.Handlers;
using RealtorApi.Infrastructure.Persistence;
using RealtorApi.Infrastructure.Storage;

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
        services.AddSingleton<IPropertyImageStorage, StubPropertyImageStorage>();
        services.RegisterHandlers(typeof(ListPropertiesHandler).Assembly);

        using var provider = services.BuildServiceProvider();
        var handlers = provider.GetServices<IHandler>();

        Assert.Contains(handlers, handler => handler is ListPropertiesHandler);
    }

    private sealed class StubPropertyImageStorage : IPropertyImageStorage
    {
        public Task<string> StoreAsync(IFormFile image, CancellationToken cancellationToken) =>
            Task.FromResult("/assets/properties/stub.png");

        public Task RemoveAsync(string imageUrl, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }
}
