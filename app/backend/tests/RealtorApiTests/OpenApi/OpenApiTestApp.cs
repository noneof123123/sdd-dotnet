using Microsoft.AspNetCore.Builder;
using RealtorApi.Features.Health;
using RealtorApi.Infrastructure.Endpoints;
using RealtorApi.Infrastructure.Handlers;

namespace RealtorApiTests.OpenApi;

public static class OpenApiTestApp
{
    public static string RepositoryRoot { get; } = FindRepositoryRoot();

    public static string ApiProjectRoot { get; } = Path.Combine(
        RepositoryRoot,
        "app",
        "backend",
        "src",
        "RealtorApi");

    public static string DocumentPath { get; } = Path.Combine(
        ApiProjectRoot,
        "wwwroot",
        "openapi",
        "v1.json");

    public static WebApplication CreateEndpointInventoryApp()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.RegisterSlices(typeof(HealthSlice).Assembly);
        builder.Services.RegisterHandlers(typeof(HealthSlice).Assembly);

        var app = builder.Build();
        app.MapSliceEndpoints();
        return app;
    }

    public static WebApplication CreateStaticFileApp()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ContentRootPath = ApiProjectRoot,
            EnvironmentName = "Production"
        });

        var app = builder.Build();
        app.UseStaticFiles();
        return app;
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            var solutionPath = Path.Combine(directory.FullName, "app", "Realtor.sln");
            if (File.Exists(solutionPath))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("No se encontró la raíz del repositorio desde " + AppContext.BaseDirectory);
    }
}
