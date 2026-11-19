using Microsoft.EntityFrameworkCore;
using RealtorApi.Domain.Properties;
using RealtorApi.Infrastructure.Persistence;

namespace RealtorApiTests.Infrastructure.Persistence;

public sealed class PropertyPersistenceTests
{
    [Fact]
    public void PropertyStatus_IsConfiguredAsStringConversion()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new AppDbContext(options);
        var entity = context.Model.FindEntityType(typeof(Property));
        var status = entity!.FindProperty(nameof(Property.Status));

        Assert.NotNull(status);
        Assert.Equal(typeof(PropertyStatus), status!.ClrType);
    }

    [Fact]
    public void PropertyConfiguration_AppliesRequiredLengthsAndPrecision()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new AppDbContext(options);
        var entity = context.Model.FindEntityType(typeof(Property));

        Assert.NotNull(entity);
        var title = entity!.FindProperty(nameof(Property.Title));
        var description = entity.FindProperty(nameof(Property.Description));
        var address = entity.FindProperty(nameof(Property.Address));
        var price = entity.FindProperty(nameof(Property.Price));
        Assert.NotNull(title);
        Assert.NotNull(description);
        Assert.NotNull(address);
        Assert.NotNull(price);

        Assert.Equal("properties", entity.GetTableName());
        Assert.Equal(200, title!.GetMaxLength());
        Assert.Equal(4000, description!.GetMaxLength());
        Assert.Equal(500, address!.GetMaxLength());
        Assert.Equal(18, price!.GetPrecision());
        Assert.Equal(2, price.GetScale());
    }

    [Fact]
    public async Task DatabaseSeederAsync_IsIdempotent()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDbContext(options);
        await DatabaseSeeder.SeedAsync(context);
        await DatabaseSeeder.SeedAsync(context);

        Assert.Equal(10, await context.Properties.CountAsync());
        Assert.Equal(3, Enum.GetNames<PropertyStatus>().Length);
    }

    [Fact]
    public void MigrationSources_ExistAsASetOfExactlyOne()
    {
        var repositoryRoot = FindRepositoryRoot();
        var migrationDirectory = Path.Combine(
            repositoryRoot,
            "app", "backend", "src", "RealtorApi", "Infrastructure", "Persistence", "Migrations");

        var migrations = Directory.GetFiles(migrationDirectory, "*_*.cs")
            .Where(path => !path.EndsWith("Snapshot.cs", StringComparison.Ordinal))
            .Where(path => !path.EndsWith(".Designer.cs", StringComparison.Ordinal))
            .ToArray();

        Assert.Single(migrations);
        Assert.Contains("CreateTable", File.ReadAllText(migrations[0]), StringComparison.Ordinal);
        Assert.Contains("Down", File.ReadAllText(migrations[0]), StringComparison.Ordinal);
    }

    [Fact]
    public void SeedManifest_AndPublicImageFiles_AreAvailable()
    {
        var repositoryRoot = FindRepositoryRoot();
        var seedDirectory = Path.Combine(repositoryRoot, "support", "seed-data");

        Assert.True(File.Exists(Path.Combine(seedDirectory, "seed-manifest.json")));
        Assert.True(File.Exists(Path.Combine(seedDirectory, "images", "properties.json")));
        Assert.True(File.Exists(Path.Combine(seedDirectory, "images", "properties-statuses.json")));
        Assert.Equal(10, Directory.GetFiles(Path.Combine(seedDirectory, "images", "properties"), "*.png").Length);
    }

    [Fact]
    public void StartupConfiguration_IncludesNativeSeedingAndMigrationCall()
    {
        var repositoryRoot = FindRepositoryRoot();
        var programSource = File.ReadAllText(Path.Combine(repositoryRoot, "app", "backend", "src", "RealtorApi", "Program.cs"));

        Assert.Contains("UseSeeding", programSource, StringComparison.Ordinal);
        Assert.Contains("UseAsyncSeeding", programSource, StringComparison.Ordinal);
        Assert.Contains("await app.MigrateAsync();", programSource, StringComparison.Ordinal);
    }

    [Fact]
    public void Architecture_DoesNotAddParallelBusinessEndpoints()
    {
        var repositoryRoot = FindRepositoryRoot();
        var backendSource = Path.Combine(repositoryRoot, "app", "backend", "src", "RealtorApi");
        var controllers = Directory.GetFiles(backendSource, "*Controller*.cs", SearchOption.AllDirectories);

        Assert.Empty(controllers);
        Assert.True(Directory.Exists(Path.Combine(backendSource, "Features", "Properties")));
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "support", "seed-data")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("No se encontró la raíz del repositorio.");
    }
}
