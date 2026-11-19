using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RealtorApi.Infrastructure.Endpoints;
using RealtorApi.Infrastructure.Handlers;
using RealtorApi.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.RegisterSlices(typeof(Program).Assembly);
builder.Services.RegisterHandlers(typeof(Program).Assembly);
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly, ServiceLifetime.Scoped);
builder.Services.AddDbContext<AppDbContext>(options =>
    options
        .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
        .UseSeeding((context, _) => DatabaseSeeder.Seed((AppDbContext)context))
        .UseAsyncSeeding(async (context, _, cancellationToken) =>
            await DatabaseSeeder.SeedAsync((AppDbContext)context, cancellationToken)));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.MapSliceEndpoints();

await app.MigrateAsync();

app.Run();
