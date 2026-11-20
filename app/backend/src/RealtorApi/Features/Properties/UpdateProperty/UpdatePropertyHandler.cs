using Microsoft.EntityFrameworkCore;
using RealtorApi.Domain.Properties;
using RealtorApi.Infrastructure.Handlers;
using RealtorApi.Infrastructure.Persistence;
using RealtorApi.Infrastructure.Results;
using RealtorApi.Infrastructure.Storage;

namespace RealtorApi.Features.Properties.UpdateProperty;

public sealed class UpdatePropertyHandler(AppDbContext context, IPropertyImageStorage imageStorage) : IHandler
{
    public async Task<Result<UpdatePropertyResponse>> HandleAsync(
        UpdatePropertyRequest request,
        CancellationToken cancellationToken)
    {
        var property = await context.Properties.FirstOrDefaultAsync(
            candidate => candidate.Id == request.Id,
            cancellationToken);

        if (property is null)
            return Result<UpdatePropertyResponse>.Failure(
                Error.NotFound("property_not_found", "No existe una propiedad con el identificador indicado."));

        var snapshot = PropertySnapshot.From(property);
        var persisted = false;
        string? newImageUrl = null;

        try
        {
            if (request.Image is not null)
                newImageUrl = await imageStorage.StoreAsync(request.Image, cancellationToken);

            property.Title = request.Title;
            property.Description = request.Description;
            property.Address = request.Address;
            property.Price = request.Price;
            property.Status = request.Status;
            property.BedroomCount = request.BedroomCount;
            property.BathroomCount = request.BathroomCount;
            property.AreaSquareMeters = request.AreaSquareMeters;
            if (newImageUrl is not null)
                property.ImageUrl = newImageUrl;
            property.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);
            persisted = true;

            if (newImageUrl is not null && snapshot.ImageUrl is not null)
                await imageStorage.RemoveAsync(snapshot.ImageUrl, cancellationToken);

            return Result<UpdatePropertyResponse>.Success(UpdatePropertyMapping.ToResponse(property));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            await CompensateAsync(property, snapshot, newImageUrl, persisted);
            throw;
        }
        catch
        {
            await CompensateAsync(property, snapshot, newImageUrl, persisted);
            return Result<UpdatePropertyResponse>.Failure(Error.Internal());
        }
    }

    private async Task CompensateAsync(
        Property property,
        PropertySnapshot snapshot,
        string? newImageUrl,
        bool persisted)
    {
        snapshot.Restore(property);

        if (persisted)
        {
            try
            {
                await context.SaveChangesAsync(CancellationToken.None);
            }
            catch
            {
            }
        }

        if (newImageUrl is null)
            return;

        try
        {
            await imageStorage.RemoveAsync(newImageUrl, CancellationToken.None);
        }
        catch
        {
        }
    }

    private sealed record PropertySnapshot(
        Guid Id,
        string Title,
        string Description,
        string Address,
        decimal Price,
        PropertyStatus Status,
        int BedroomCount,
        int BathroomCount,
        decimal AreaSquareMeters,
        string? ImageUrl,
        DateTime CreatedAt,
        DateTime? UpdatedAt)
    {
        public static PropertySnapshot From(Property property) => new(
            property.Id,
            property.Title,
            property.Description,
            property.Address,
            property.Price,
            property.Status,
            property.BedroomCount,
            property.BathroomCount,
            property.AreaSquareMeters,
            property.ImageUrl,
            property.CreatedAt,
            property.UpdatedAt);

        public void Restore(Property property)
        {
            property.Id = Id;
            property.Title = Title;
            property.Description = Description;
            property.Address = Address;
            property.Price = Price;
            property.Status = Status;
            property.BedroomCount = BedroomCount;
            property.BathroomCount = BathroomCount;
            property.AreaSquareMeters = AreaSquareMeters;
            property.ImageUrl = ImageUrl;
            property.CreatedAt = CreatedAt;
            property.UpdatedAt = UpdatedAt;
        }
    }
}
