namespace RealtorApi.Features.Properties.ListProperties;

public sealed record ListPropertiesRequest
{
    public int? Page { get; init; }

    public int? PageSize { get; init; }
}
