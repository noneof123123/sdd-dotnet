using Microsoft.Extensions.DependencyInjection;
using RealtorApi.Infrastructure.Handlers;

namespace RealtorApiTests.Infrastructure.Handlers;

public sealed class RegisterHandlersTests
{
    [Fact]
    public void RegisterHandlers_DetectsTestHandlerWithoutDuplicates()
    {
        var services = new ServiceCollection();
        services.RegisterHandlers(typeof(TestHandler).Assembly);

        var handlers = services.Where(descriptor => descriptor.ServiceType == typeof(IHandler)).ToArray();

        Assert.Contains(handlers, descriptor => descriptor.ImplementationType == typeof(TestHandler));
        Assert.Single(handlers, descriptor => descriptor.ImplementationType == typeof(TestHandler));
    }
}
