using RealtorApi.Domain.Properties;
using RealtorApi.Infrastructure.Handlers;
using RealtorApi.Infrastructure.Persistence;
using RealtorApi.Infrastructure.Results;
using RealtorApi.Infrastructure.Storage;

namespace RealtorApi.Features.Properties.CreateProperty;

public sealed class CreatePropertyHandler(AppDbContext context, IPropertyImageStorage imageStorage) : IHandler
{
    public async Task<Result<CreatePropertyResponse>> HandleAsync(
        CreatePropertyRequest request,
        CancellationToken cancellationToken)
    {
        string? storedImageUrl = null;

        try
        {
            if (request.Image is not null)
                storedImageUrl = await imageStorage.StoreAsync(request.Image, cancellationToken);

            var property = new Property
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Description = request.Description,
                Address = request.Address,
                Price = request.Price,
                Status = request.Status,
                BedroomCount = request.BedroomCount,
                BathroomCount = request.BathroomCount,
                AreaSquareMeters = request.AreaSquareMeters,
                ImageUrl = storedImageUrl,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            context.Properties.Add(property);
            await context.SaveChangesAsync(cancellationToken);

            return Result<CreatePropertyResponse>.Success(CreatePropertyMapping.ToResponse(property));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            await TryRemoveStoredImageAsync(storedImageUrl);
            throw;
        }
        catch
        {
            await TryRemoveStoredImageAsync(storedImageUrl);
            return Result<CreatePropertyResponse>.Failure(Error.Internal());
        }
    }

    private async Task TryRemoveStoredImageAsync(string? imageUrl)
    {
        if (imageUrl is null)
            return;

        try
        {
            await imageStorage.RemoveAsync(imageUrl, CancellationToken.None);
        }
        catch
        {
        }
    }
}
