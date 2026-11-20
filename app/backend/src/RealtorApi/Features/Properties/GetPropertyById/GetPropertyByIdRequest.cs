using Microsoft.AspNetCore.Mvc;

namespace RealtorApi.Features.Properties.GetPropertyById;

public sealed record GetPropertyByIdRequest
{
    [FromRoute(Name = "id")]
    public Guid Id { get; init; }
}
