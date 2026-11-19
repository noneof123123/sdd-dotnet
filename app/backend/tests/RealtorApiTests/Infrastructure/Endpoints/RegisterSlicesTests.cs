using Microsoft.Extensions.DependencyInjection;
using RealtorApi.Infrastructure.Endpoints;

namespace RealtorApiTests.Infrastructure.Endpoints;

public sealed class RegisterSlicesTests
{
    [Fact]
    public void RegisterSlices_DetectsTestSliceWithoutDuplicates()
    {
        var services = new ServiceCollection();
        services.RegisterSlices(typeof(TestSlice).Assembly);

        var slices = services.Where(descriptor => descriptor.ServiceType == typeof(ISlice)).ToArray();

        Assert.Contains(slices, descriptor => descriptor.ImplementationType == typeof(TestSlice));
        Assert.Single(slices, descriptor => descriptor.ImplementationType == typeof(TestSlice));
    }
}
