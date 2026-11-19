using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RealtorApi.Domain.Properties;

namespace RealtorApi.Infrastructure.Persistence;

public sealed record SeedManifest(
    string PropertiesFile,
    string StatusesFile,
    string ImagesSourceDirectory,
    string ImagesPublicDirectory,
    string ImagesPublicBaseUrl);

public static class DatabaseSeeder
{
    private sealed record PropertySeed(
        Guid Id,
        string Title,
        string Description,
        string Address,
        decimal Price,
        string Status,
        int BedroomCount,
        int BathroomCount,
        decimal AreaSquareMeters,
        string ImageUrl,
        DateTime CreatedAt,
        DateTime? UpdatedAt);

    private sealed record StatusSeed(string Value);

    public static void Seed(AppDbContext context)
    {
        SeedCore(context, null).GetAwaiter().GetResult();
    }

    public static async Task SeedAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        await SeedCore(context, cancellationToken);
    }

    private static async Task SeedCore(AppDbContext context, CancellationToken? cancellationToken)
    {
        var ct = cancellationToken ?? CancellationToken.None;
        var baseDirectory = ResolveSeedDirectory();
        var manifestPath = Path.Combine(baseDirectory, "seed-manifest.json");
        if (!File.Exists(manifestPath))
        {
            throw new InvalidOperationException($"No se encontró el manifiesto de seed: {manifestPath}");
        }

        var manifest = await ReadJsonAsync<SeedManifest>(manifestPath, ct);
        var propertiesPath = Path.Combine(baseDirectory, manifest.PropertiesFile);
        var statusesPath = Path.Combine(baseDirectory, manifest.StatusesFile);
        if (!File.Exists(propertiesPath) || !File.Exists(statusesPath))
        {
            throw new InvalidOperationException("Faltan los archivos JSON declarados por el manifiesto de seed.");
        }

        var statuses = await ReadJsonAsync<StatusSeed[]>(statusesPath, ct);
        var validStatuses = Enum.GetNames<PropertyStatus>().ToHashSet(StringComparer.Ordinal);
        if (statuses.Any(status => !validStatuses.Contains(status.Value)))
        {
            throw new InvalidOperationException("El manifiesto contiene estados externos al catálogo PropertyStatus.");
        }

        var sourceImages = Path.Combine(baseDirectory, manifest.ImagesSourceDirectory);
        var publicDirectory = ResolvePublicDirectory(manifest.ImagesPublicDirectory);
        Directory.CreateDirectory(publicDirectory);

        var properties = await ReadJsonAsync<PropertySeed[]>(propertiesPath, ct);
        foreach (var item in properties)
        {
            if (!Enum.TryParse<PropertyStatus>(item.Status, ignoreCase: false, out var status))
            {
                throw new InvalidOperationException($"Estado inválido en la propiedad seed {item.Id}: {item.Status}");
            }

            var sourceImage = Path.Combine(sourceImages, item.ImageUrl);
            if (!File.Exists(sourceImage))
            {
                throw new InvalidOperationException($"Falta la imagen seed declarada: {sourceImage}");
            }

            var fileName = Path.GetFileName(item.ImageUrl);
            await CopyFileAsync(sourceImage, Path.Combine(publicDirectory, fileName), ct);

            var existing = await context.Properties.FirstOrDefaultAsync(property => property.Id == item.Id, ct);
            var publicUrl = $"{manifest.ImagesPublicBaseUrl.TrimEnd('/')}/{fileName}";
            if (existing is null)
            {
                context.Properties.Add(new Property
                {
                    Id = item.Id,
                    Title = item.Title,
                    Description = item.Description,
                    Address = item.Address,
                    Price = item.Price,
                    Status = status,
                    BedroomCount = item.BedroomCount,
                    BathroomCount = item.BathroomCount,
                    AreaSquareMeters = item.AreaSquareMeters,
                    ImageUrl = publicUrl,
                    CreatedAt = item.CreatedAt,
                    UpdatedAt = item.UpdatedAt
                });
            }
            else
            {
                existing.Title = item.Title;
                existing.Description = item.Description;
                existing.Address = item.Address;
                existing.Price = item.Price;
                existing.Status = status;
                existing.BedroomCount = item.BedroomCount;
                existing.BathroomCount = item.BathroomCount;
                existing.AreaSquareMeters = item.AreaSquareMeters;
                existing.ImageUrl = publicUrl;
                existing.CreatedAt = item.CreatedAt;
                existing.UpdatedAt = item.UpdatedAt;
            }
        }

        await context.SaveChangesAsync(ct);
    }

    private static async Task<T> ReadJsonAsync<T>(string path, CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<T>(stream,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, cancellationToken)
            ?? throw new InvalidOperationException($"No se pudo deserializar: {path}");
    }

    private static async Task CopyFileAsync(string source, string destination, CancellationToken cancellationToken)
    {
        await using var input = File.OpenRead(source);
        await using var output = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);
        await input.CopyToAsync(output, cancellationToken);
    }

    private static string ResolveSeedDirectory()
    {
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "seed-data"),
            Path.Combine(AppContext.BaseDirectory, "support", "seed-data"),
            Path.Combine(Directory.GetCurrentDirectory(), "support", "seed-data")
        };
        return candidates.FirstOrDefault(Directory.Exists) ?? candidates[0];
    }

    private static string ResolvePublicDirectory(string manifestDirectory)
    {
        if (Path.IsPathRooted(manifestDirectory))
        {
            return manifestDirectory;
        }

        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, manifestDirectory),
            Path.Combine(Directory.GetCurrentDirectory(), "app", "backend", "src", "RealtorApi", manifestDirectory),
            Path.Combine(Directory.GetCurrentDirectory(), manifestDirectory)
        };
        var matched = candidates.FirstOrDefault(candidate => candidate.Contains("wwwroot", StringComparison.Ordinal));
        return matched ?? candidates[0];
    }
}
