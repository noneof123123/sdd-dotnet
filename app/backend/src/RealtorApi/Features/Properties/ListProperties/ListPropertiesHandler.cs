using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RealtorApi.Infrastructure.Handlers;
using RealtorApi.Infrastructure.Persistence;

namespace RealtorApi.Features.Properties.ListProperties;

public sealed class ListPropertiesHandler(AppDbContext context) : IHandler
{
    public async Task<ListPropertiesResponse> HandleAsync(
        ListPropertiesRequest request,
        HttpRequest httpRequest,
        CancellationToken cancellationToken)
    {
        var pageSize = Math.Min(Math.Max(request.PageSize ?? 6, 1), 24);
        var totalItems = await context.Properties
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var totalPages = totalItems == 0
            ? 0
            : (int)Math.Ceiling(totalItems / (double)pageSize);

        var page = totalPages == 0
            ? 1
            : Math.Min(Math.Max(request.Page ?? 1, 1), totalPages);

        var items = await context.Properties
            .AsNoTracking()
            .OrderBy(property => property.Title)
            .ThenBy(property => property.Address)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(property => new
            {
                property.Id,
                property.Title,
                property.Description,
                property.Address,
                property.Price,
                Status = property.Status.ToString(),
                property.BedroomCount,
                property.BathroomCount,
                property.AreaSquareMeters,
                property.ImageUrl
            })
            .ToListAsync(cancellationToken);

        var responseItems = items
            .Select(item => ListPropertiesMapping.ToResponse(
                item.Id,
                item.Title,
                item.Description,
                item.Address,
                item.Price,
                item.Status,
                item.BedroomCount,
                item.BathroomCount,
                item.AreaSquareMeters,
                item.ImageUrl,
                httpRequest))
            .ToList();

        return new ListPropertiesResponse(
            responseItems,
            page,
            pageSize,
            totalItems,
            totalPages,
            page < totalPages,
            totalPages > 0 && page > 1);
    }
}
