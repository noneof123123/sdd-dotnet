using Microsoft.AspNetCore.Mvc;

namespace RealtorApi.Features.Properties.ListProperties;

public sealed record ListPropertiesRequest
{
    [FromQuery(Name = "page")]
    public int? Page { get; init; }

    [FromQuery(Name = "pageSize")]
    public int? PageSize { get; init; }
}
