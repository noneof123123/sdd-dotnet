using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RealtorApi.Domain.Properties;
using RealtorApi.Infrastructure.Handlers;
using RealtorApi.Infrastructure.Logging;
using RealtorApi.Infrastructure.Persistence;
using RealtorApi.Infrastructure.Results;

namespace RealtorApi.Features.Properties.ChangePropertyStatus;

public sealed class ChangePropertyStatusHandler(
    AppDbContext context,
    ILogger<ChangePropertyStatusHandler>? logger = null) : IHandler
{
    public async Task<Result<ChangePropertyStatusResponse>> HandleAsync(
        ChangePropertyStatusRequest request,
        HttpRequest httpRequest,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var propertyId))
            return Result<ChangePropertyStatusResponse>.Failure(Error.Validation(
                nameof(ChangePropertyStatusRequest.Id),
                "El identificador de la propiedad debe ser un GUID válido."));

        if (!TryParseStatus(request.Body?.Status, out var requestedStatus))
            return Result<ChangePropertyStatusResponse>.Failure(Error.Validation(
                nameof(ChangePropertyStatusBody.Status),
                "El estado debe ser Available, Rented o Maintenance."));

        Property? property = null;
        var originalStatus = default(PropertyStatus);
        DateTime? originalUpdatedAt = null;

        try
        {
            property = await context.Properties.FirstOrDefaultAsync(
                candidate => candidate.Id == propertyId,
                cancellationToken);

            if (property is null)
                return Result<ChangePropertyStatusResponse>.Failure(
                    Error.NotFound("property_not_found", "No existe una propiedad con el identificador indicado."));

            originalStatus = property.Status;
            originalUpdatedAt = property.UpdatedAt;
            var statusText = requestedStatus.ToString();

            if (property.Status != requestedStatus)
            {
                property.Status = requestedStatus;
                property.UpdatedAt = DateTime.UtcNow;
                await context.SaveChangesAsync(cancellationToken);
            }

            if (logger is not null)
                InfrastructureLog.PropertyStatusChanged(logger, property.Id, statusText);

            return Result<ChangePropertyStatusResponse>.Success(
                ChangePropertyStatusMapping.ToResponse(property, httpRequest));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            Restore(property, originalStatus, originalUpdatedAt);
            throw;
        }
        catch
        {
            Restore(property, originalStatus, originalUpdatedAt);
            return Result<ChangePropertyStatusResponse>.Failure(Error.Internal());
        }
    }

    private static void Restore(Property? property, PropertyStatus status, DateTime? updatedAt)
    {
        if (property is null)
            return;

        property.Status = status;
        property.UpdatedAt = updatedAt;
    }

    private static bool TryParseStatus(string? value, out PropertyStatus status)
    {
        switch (value)
        {
            case nameof(PropertyStatus.Available):
                status = PropertyStatus.Available;
                return true;
            case nameof(PropertyStatus.Rented):
                status = PropertyStatus.Rented;
                return true;
            case nameof(PropertyStatus.Maintenance):
                status = PropertyStatus.Maintenance;
                return true;
            default:
                status = default;
                return false;
        }
    }
}
