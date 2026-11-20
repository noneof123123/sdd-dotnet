using System.Text.Json.Serialization;
using Refit;

namespace RealtorWeb.Features.Properties;

public static class PropertyStatus
{
    public const string Available = "Available";
    public const string Rented = "Rented";
    public const string Maintenance = "Maintenance";
}

public sealed class ListPropertiesItemResponse
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; init; } = string.Empty;

    [JsonPropertyName("address")]
    public string Address { get; init; } = string.Empty;

    [JsonPropertyName("price")]
    public decimal Price { get; init; }

    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;

    [JsonPropertyName("bedroomCount")]
    public int BedroomCount { get; init; }

    [JsonPropertyName("bathroomCount")]
    public int BathroomCount { get; init; }

    [JsonPropertyName("areaSquareMeters")]
    public int AreaSquareMeters { get; init; }

    [JsonPropertyName("imageUrl")]
    public string? ImageUrl { get; init; }
}

public sealed class ListPropertiesResponse
{
    [JsonPropertyName("items")]
    public IReadOnlyList<ListPropertiesItemResponse> Items { get; init; } = [];

    [JsonPropertyName("page")]
    public int Page { get; init; }

    [JsonPropertyName("pageSize")]
    public int PageSize { get; init; }

    [JsonPropertyName("totalItems")]
    public int TotalItems { get; init; }

    [JsonPropertyName("totalPages")]
    public int TotalPages { get; init; }

    [JsonPropertyName("hasNext")]
    public bool HasNext { get; init; }

    [JsonPropertyName("hasPrevious")]
    public bool HasPrevious { get; init; }
}

public sealed class GetPropertyByIdResponse
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; init; } = string.Empty;

    [JsonPropertyName("address")]
    public string Address { get; init; } = string.Empty;

    [JsonPropertyName("price")]
    public decimal Price { get; init; }

    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;

    [JsonPropertyName("bedroomCount")]
    public int BedroomCount { get; init; }

    [JsonPropertyName("bathroomCount")]
    public int BathroomCount { get; init; }

    [JsonPropertyName("areaSquareMeters")]
    public int AreaSquareMeters { get; init; }

    [JsonPropertyName("imageUrl")]
    public string? ImageUrl { get; init; }

    [JsonPropertyName("createdAt")]
    public DateTimeOffset CreatedAt { get; init; }

    [JsonPropertyName("updatedAt")]
    public DateTimeOffset? UpdatedAt { get; init; }
}

public interface IPropertiesApi
{
    [Get("/api/properties")]
    Task<ApiResponse<ListPropertiesResponse>> ListAsync(
        [Query] int page,
        [Query] int pageSize,
        CancellationToken cancellationToken);

    [Get("/api/properties/{id}")]
    Task<ApiResponse<GetPropertyByIdResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);
}
