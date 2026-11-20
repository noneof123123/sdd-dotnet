using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using RealtorApi.Infrastructure.Storage;

namespace RealtorApiTests.Features.Properties.CreateProperty;

public sealed class PropertyImageStorageTests : IDisposable
{
    private readonly string _rootPath;

    public PropertyImageStorageTests()
    {
        _rootPath = Path.Combine(Path.GetTempPath(), $"realtor-storage-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Path.Combine(_rootPath, "wwwroot", "assets", "properties"));
    }

    [Fact]
    public async Task StoreAsync_WithValidatedPng_WritesUniqueFileAndReturnsRelativeUrl()
    {
        var storage = CreateStorage();
        var first = CreateImage("original.png", "image/png");
        var second = CreateImage("original.png", "image/png");

        var firstUrl = await storage.StoreAsync(first, CancellationToken.None);
        var secondUrl = await storage.StoreAsync(second, CancellationToken.None);

        Assert.StartsWith("/assets/properties/", firstUrl);
        Assert.EndsWith(".png", firstUrl, StringComparison.Ordinal);
        Assert.NotEqual(firstUrl, secondUrl);
        Assert.True(File.Exists(ResolvePath(firstUrl)));
        Assert.True(File.Exists(ResolvePath(secondUrl)));
        Assert.DoesNotContain("original", firstUrl, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task StoreAsync_WithValidatedJpeg_PreservesValidatedExtension()
    {
        var storage = CreateStorage();

        var url = await storage.StoreAsync(CreateImage("photo.jpeg", "image/jpeg"), CancellationToken.None);

        Assert.EndsWith(".jpeg", url, StringComparison.Ordinal);
        Assert.True(File.Exists(ResolvePath(url)));
    }

    [Fact]
    public async Task RemoveAsync_DeletesOnlyRequestedFile()
    {
        var storage = CreateStorage();
        var url = await storage.StoreAsync(CreateImage("photo.png", "image/png"), CancellationToken.None);

        await storage.RemoveAsync(url, CancellationToken.None);

        Assert.False(File.Exists(ResolvePath(url)));
    }

    [Fact]
    public async Task StoreAsync_WhenDirectoryIsUnavailable_ThrowsInfrastructureError()
    {
        var storage = CreateStorage();
        var directory = Path.Combine(_rootPath, "wwwroot", "assets", "properties");
        Directory.Delete(directory, recursive: true);
        File.WriteAllText(directory, "No es un directorio.");

        await Assert.ThrowsAnyAsync<IOException>(
            () => storage.StoreAsync(CreateImage("photo.png", "image/png"), CancellationToken.None));
    }

    public void Dispose()
    {
        var directory = Path.Combine(_rootPath, "wwwroot", "assets", "properties");
        if (File.Exists(directory))
            File.Delete(directory);
        if (Directory.Exists(_rootPath))
            Directory.Delete(_rootPath, recursive: true);
    }

    private PropertyImageStorage CreateStorage() => new(new TestEnvironment
    {
        ContentRootPath = _rootPath
    });

    private string ResolvePath(string relativeUrl)
    {
        var fileName = Path.GetFileName(relativeUrl);
        return Path.Combine(_rootPath, "wwwroot", "assets", "properties", fileName);
    }

    private static FormFile CreateImage(string fileName, string contentType)
    {
        byte[] bytes = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x01, 0x02];
        return new FormFile(new MemoryStream(bytes), 0, bytes.Length, "image", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }

    private sealed class TestEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "RealtorApiTests";

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();

        public string ContentRootPath { get; set; } = string.Empty;

        public string EnvironmentName { get; set; } = "Development";

        public string WebRootPath { get; set; } = string.Empty;

        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    }
}
