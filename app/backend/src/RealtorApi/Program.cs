using System.Reflection;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi;
using RealtorApi.Infrastructure.Endpoints;
using RealtorApi.Infrastructure.Handlers;
using RealtorApi.Infrastructure.OpenApi;
using RealtorApi.Infrastructure.Persistence;
using RealtorApi.Infrastructure.Storage;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi(RealtorOpenApiConstants.DocumentName, options =>
{
    options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_0;
    options.AddDocumentTransformer<OpenApiDocumentTransformer>();
    options.AddDocumentTransformer<OpenApiSchemaCatalog>();
});
builder.Services.RegisterSlices(typeof(Program).Assembly);
builder.Services.RegisterHandlers(typeof(Program).Assembly);
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly, ServiceLifetime.Scoped);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IPropertyImageStorage, PropertyImageStorage>();
builder.Services.AddDbContext<AppDbContext>(options =>
    options
        .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
        .UseSeeding((context, _) => DatabaseSeeder.Seed((AppDbContext)context))
        .UseAsyncSeeding(async (context, _, cancellationToken) =>
            await DatabaseSeeder.SeedAsync((AppDbContext)context, cancellationToken)));

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.UseStaticFiles();
var assetsDirectoryCandidates = new[]
{
    Path.Combine(app.Environment.ContentRootPath, "wwwroot", "assets", "properties"),
    Path.Combine(AppContext.BaseDirectory, "wwwroot", "assets", "properties"),
    Path.Combine(AppContext.BaseDirectory, "seed-data", "images", "properties"),
    Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, "..", "..", "..", "..", "support", "seed-data", "images", "properties"))
};
var assetsDirectory = assetsDirectoryCandidates.First(Directory.Exists);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(assetsDirectory),
    RequestPath = "/assets/properties"
});
app.MapSliceEndpoints();

var entryAssemblyName = Assembly.GetEntryAssembly()?.GetName().Name;
var isDocumentGeneration = string.Equals(entryAssemblyName, "GetDocument.Insider", StringComparison.OrdinalIgnoreCase)
    || string.Equals(entryAssemblyName, "Microsoft.Extensions.ApiDescription.Server", StringComparison.OrdinalIgnoreCase);

if (!isDocumentGeneration)
{
    await app.MigrateAsync();
}

app.Run();
