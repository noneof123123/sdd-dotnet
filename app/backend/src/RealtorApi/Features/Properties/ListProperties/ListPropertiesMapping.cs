using Microsoft.AspNetCore.Http;

namespace RealtorApi.Features.Properties.ListProperties;

public static class ListPropertiesMapping
{
    public static ListPropertiesItemResponse ToResponse(
        Guid id,
        string title,
        string description,
        string address,
        decimal price,
        string status,
        int bedroomCount,
        int bathroomCount,
        decimal areaSquareMeters,
        string persistedImageUrl,
        HttpRequest request)
    {
        var fileName = ResolveFileName(persistedImageUrl);
        var imageUrl = fileName.Length == 0
            ? string.Empty
            : $"{request.Scheme}://{request.Host}/assets/properties/{fileName}";

        return new ListPropertiesItemResponse(
            id,
            title,
            description,
            address,
            price,
            status,
            bedroomCount,
            bathroomCount,
            areaSquareMeters,
            imageUrl);
    }

    private static string ResolveFileName(string persistedImageUrl)
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
