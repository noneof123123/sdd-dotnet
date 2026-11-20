using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RealtorApi.Features.Properties.ChangePropertyStatus;

public sealed record ChangePropertyStatusRequest(
    [property: FromRoute(Name = "id")] string? Id = null,
    ChangePropertyStatusBody? Body = null) : IBindableFromHttpContext<ChangePropertyStatusRequest>
{
    public static async ValueTask<ChangePropertyStatusRequest?> BindAsync(
        HttpContext context,
        ParameterInfo parameter)
    {
        string? id = context.Request.RouteValues.TryGetValue("id", out var value)
            ? value?.ToString()
            : null;
        ChangePropertyStatusBody? body = null;

        if (context.Request.ContentLength is > 0)
        {
            try
            {
                body = await context.Request.ReadFromJsonAsync<ChangePropertyStatusBody>(
                    cancellationToken: context.RequestAborted);
            }
            catch (JsonException)
            {
                body = null;
            }
        }

        return new ChangePropertyStatusRequest(id, body);
    }
}

public sealed record ChangePropertyStatusBody
{
    public string? Status { get; init; }
}
