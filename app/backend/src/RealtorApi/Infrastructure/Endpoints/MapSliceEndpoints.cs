using RealtorApi.Infrastructure.Validation;

namespace RealtorApi.Infrastructure.Endpoints;

public static class SliceMapping
{
    public static IEndpointRouteBuilder MapSliceEndpoints(this IEndpointRouteBuilder app)
    {
        var routeGroup = app.MapGroup(string.Empty);

        routeGroup.AddEndpointFilterFactory((factoryContext, next) =>
        {
            var requestTypes = factoryContext.MethodInfo.GetParameters()
                .Select(parameter => parameter.ParameterType)
                .Where(type => type != typeof(CancellationToken) && type != typeof(HttpContext))
                .Distinct()
                .ToArray();

            var filters = requestTypes
                .Select(ValidationFilterFactory.Create)
                .Cast<IEndpointFilter>()
                .ToArray();

            return async invocationContext =>
            {
                EndpointFilterDelegate pipeline = next;

                foreach (var filter in filters.Reverse())
                {
                    var current = filter;
                    var previous = pipeline;
                    pipeline = context => current.InvokeAsync(context, previous);
                }

                return await pipeline(invocationContext);
            };
        });

        var slices = app.ServiceProvider.GetServices<ISlice>();

        foreach (var slice in slices)
            slice.AddEndpoint(routeGroup);

        return app;
    }
}
