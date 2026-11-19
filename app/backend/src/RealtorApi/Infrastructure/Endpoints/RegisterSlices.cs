using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace RealtorApi.Infrastructure.Endpoints;

public static class SliceRegistration
{
    public static IServiceCollection RegisterSlices(this IServiceCollection services, Assembly assembly)
    {
        var sliceTypes = assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } && typeof(ISlice).IsAssignableFrom(type))
            .Distinct()
            .ToArray();

        foreach (var sliceType in sliceTypes)
            services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(ISlice), sliceType));

        return services;
    }
}
