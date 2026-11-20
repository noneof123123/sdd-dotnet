using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace RealtorApi.Infrastructure.Storage;

public sealed class PropertyImageStorage(IWebHostEnvironment environment) : IPropertyImageStorage
{
    private readonly string _directory = Path.Combine(
        environment.ContentRootPath,
        "wwwroot",
        "assets",
        "properties");

    public async Task<string> StoreAsync(IFormFile image, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(_directory);
        var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(_directory, fileName);

        await using var stream = new FileStream(
            fullPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 8192,
            useAsync: true);
        await image.CopyToAsync(stream, cancellationToken);

        return $"/assets/properties/{fileName}";
    }

    public Task RemoveAsync(string imageUrl, CancellationToken cancellationToken)
    {
        var fileName = Path.GetFileName(imageUrl);
        if (string.IsNullOrWhiteSpace(fileName) || fileName is "." or "..")
            return Task.CompletedTask;

        var fullPath = Path.Combine(_directory, fileName);
        if (File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }
}
