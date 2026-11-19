using System.Reflection;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using static Microsoft.AspNetCore.Http.Results;

namespace RealtorApi.Infrastructure.Validation;

public static class ValidationFilterFactory
{
    public static IEndpointFilter Create(Type requestType)
        => (IEndpointFilter)Activator.CreateInstance(typeof(ValidationFilter<>).MakeGenericType(requestType))!;

    private sealed class ValidationFilter<TRequest> : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var validator = context.HttpContext.RequestServices.GetService<IValidator<TRequest>>();
            if (validator is null)
                return await next(context);

            TRequest? request = default;
            foreach (var argument in context.Arguments)
            {
                if (argument is TRequest typed)
                {
                    request = typed;
                    break;
                }
            }

            if (request is null)
                return await next(context);

            var result = await validator.ValidateAsync(request, context.HttpContext.RequestAborted);
            if (result.IsValid)
                return await next(context);

            var errors = result.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray());

            return ValidationProblem(errors, statusCode: StatusCodes.Status400BadRequest);
        }
    }
}
