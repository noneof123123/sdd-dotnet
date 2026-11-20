using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RealtorApi.Infrastructure.Handlers;
using RealtorApi.Infrastructure.Logging;
using RealtorApi.Infrastructure.Persistence;
using RealtorApi.Infrastructure.Results;

namespace RealtorApi.Features.Properties.GetPropertyById;

public sealed class GetPropertyByIdHandler(
    AppDbContext context,
    ILogger<GetPropertyByIdHandler>? logger = null) : IHandler
{
    public async Task<Result<GetPropertyByIdResponse>> HandleAsync(
        GetPropertyByIdRequest request,
        HttpRequest httpRequest,
        CancellationToken cancellationToken)
    {
        if (logger is not null)
            InfrastructureLog.PropertyQueried(logger, request.Id);

        try
        {
            var property = await context.Properties
                .AsNoTracking()
                .FirstOrDefaultAsync(candidate => candidate.Id == request.Id, cancellationToken);

            if (property is null)
                return Result<GetPropertyByIdResponse>.Failure(
                    Error.NotFound("property_not_found", "No existe una propiedad con el identificador indicado."));

            return Result<GetPropertyByIdResponse>.Success(
                GetPropertyByIdMapping.ToResponse(property, httpRequest));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return Result<GetPropertyByIdResponse>.Failure(Error.Internal());
        }
    }
}
