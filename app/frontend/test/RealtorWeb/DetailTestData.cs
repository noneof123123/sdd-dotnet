using System.Net;
using RealtorWeb.Features.Properties;
using Refit;

namespace RealtorWebTests;

public static class DetailTestData
{
    public static GetPropertyByIdResponse Response(
        Guid? id = null,
        string title = "Piso luminoso",
        string description = "Descripción completa del inmueble.",
        string address = "Calle Mayor 1, Madrid",
        decimal price = 1250m,
        string status = PropertyStatus.Available,
        int bedroomCount = 3,
        int bathroomCount = 2,
        int areaSquareMeters = 95,
        string? imageUrl = "/images/piso.jpg",
        DateTimeOffset? createdAt = null,
        DateTimeOffset? updatedAt = null,
        bool hasUpdatedAt = true) =>
        new()
        {
            Id = id ?? Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Title = title,
            Description = description,
            Address = address,
            Price = price,
            Status = status,
            BedroomCount = bedroomCount,
            BathroomCount = bathroomCount,
            AreaSquareMeters = areaSquareMeters,
            ImageUrl = imageUrl,
            CreatedAt = createdAt ?? new DateTimeOffset(2026, 5, 14, 10, 30, 0, TimeSpan.Zero),
            UpdatedAt = hasUpdatedAt
                ? updatedAt ?? new DateTimeOffset(2026, 5, 20, 12, 45, 0, TimeSpan.Zero)
                : null
        };

    public static ApiResponse<T> Success<T>(T content, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var response = new HttpResponseMessage(statusCode)
        {
            RequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://example.test/api/properties")
        };
        return new ApiResponse<T>(response, content, new RefitSettings());
    }

    public static ApiResponse<GetPropertyByIdResponse> Status(HttpStatusCode statusCode)
    {
        var response = new HttpResponseMessage(statusCode)
        {
            RequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://example.test/api/properties/detalle")
        };
        return new ApiResponse<GetPropertyByIdResponse>(response, null, new RefitSettings());
    }
}

public sealed class FakeDetailApi : IPropertiesApi
{
    public Func<Guid, CancellationToken, Task<ApiResponse<GetPropertyByIdResponse>>> GetHandler { get; init; } =
        (_, _) => throw new NotSupportedException();

    public Func<int, int, Task<ApiResponse<ListPropertiesResponse>>> ListHandler { get; init; } =
        (_, _) => throw new NotSupportedException();

    public Guid RequestedId { get; private set; }

    public CancellationToken RequestedToken { get; private set; }

    public Task<ApiResponse<ListPropertiesResponse>> ListAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken) =>
        ListHandler(page, pageSize);

    public Task<ApiResponse<GetPropertyByIdResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        RequestedId = id;
        RequestedToken = cancellationToken;
        return GetHandler(id, cancellationToken);
    }
}
