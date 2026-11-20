using Microsoft.AspNetCore.Http;
using RealtorApi.Domain.Properties;

namespace RealtorApi.Features.Properties.GetPropertyById;

public static class GetPropertyByIdMapping
{
    public static GetPropertyByIdResponse ToResponse(Property property, HttpRequest request)
    {
        var fileName = ResolveFileName(property.ImageUrl);
        var imageUrl = fileName.Length == 0
            ? string.Empty
            : $"{request.Scheme}://{request.Host}/assets/properties/{fileName}";

        return new GetPropertyByIdResponse(
            property.Id,
            property.Title,
            property.Description,
            property.Address,
            property.Price,
            property.Status.ToString(),
            property.BedroomCount,
            property.BathroomCount,
            property.AreaSquareMeters,
            imageUrl,
            NormalizeUtc(property.CreatedAt),
            property.UpdatedAt is { } updatedAt ? NormalizeUtc(updatedAt) : null);
    }

    private static DateTime NormalizeUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };

    private static string ResolveFileName(string? persistedImageUrl)
    {
        if (string.IsNullOrWhiteSpace(persistedImageUrl))
            return string.Empty;

        var fileName = Path.GetFileName(persistedImageUrl.Trim());
        return fileName.Length == 0
            || fileName is "." or ".."
            || fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0
                ? string.Empty
                : fileName;
    }
}
