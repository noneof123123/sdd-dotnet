using Microsoft.EntityFrameworkCore;

namespace RealtorApi.Infrastructure.Persistence;

public static class MigrationExtensions
{
    public static async Task MigrateAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (!context.Database.IsRelational())
        {
            throw new InvalidOperationException("Esta aplicación requiere un proveedor de base de datos relacional para aplicar migraciones.");
        }

        await context.Database.MigrateAsync();
    }
}
