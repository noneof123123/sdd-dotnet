using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace RealtorApi.Infrastructure.Handlers;

public static class HandlerRegistration
{
    public static IServiceCollection RegisterHandlers(this IServiceCollection services, Assembly assembly)
    {
        var handlerTypes = assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } && typeof(IHandler).IsAssignableFrom(type))
            .Distinct()
            .ToArray();

        foreach (var handlerType in handlerTypes)
        {
            services.TryAddEnumerable(ServiceDescriptor.Scoped(typeof(IHandler), handlerType));
            services.TryAdd(ServiceDescriptor.Scoped(handlerType, handlerType));
        }

        return services;
    }
}
