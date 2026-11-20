using System.Globalization;
using System.Net;
using System.Text.Json;
using Refit;

namespace RealtorWeb.Features.Properties;

public interface IPropertyCatalogService
{
    Task<PropertyCatalogViewModel> GetCatalogAsync(int page, CancellationToken cancellationToken);

    Task<PropertyDetailsViewModel> GetDetailsAsync(Guid id, int page, CancellationToken cancellationToken);
}

public sealed class PropertyCatalogService : IPropertyCatalogService
{
    private static readonly CultureInfo Spanish = CultureInfo.GetCultureInfo("es-ES");
    private readonly IPropertiesApi _api;

    public PropertyCatalogService(IPropertiesApi api)
    {
        _api = api;
    }

    public async Task<PropertyCatalogViewModel> GetCatalogAsync(int page, CancellationToken cancellationToken)
    {
        var requestedPage = NormalizePage(page);

        try
        {
            using var response = await _api.ListAsync(requestedPage, CatalogNavigation.PageSize, cancellationToken);

            if (!response.IsSuccessStatusCode || response.Content is null)
            {
                return ErrorCatalog(requestedPage);
            }

            var content = response.Content;
            var effectivePage = NormalizePage(content.Page);
            var pageSize = Math.Max(1, content.PageSize);
            var totalItems = Math.Max(0, content.TotalItems);
            var totalPages = Math.Max(1, content.TotalPages);
            var items = content.Items.Select(item => MapCard(item, effectivePage)).ToArray();
            var state = PropertyPresentation.CatalogState(false, true, items.Length > 0);

            return new PropertyCatalogViewModel(
                items,
                effectivePage,
                pageSize,
                totalItems,
                totalPages,
                content.HasNext,
                content.HasPrevious,
                CatalogNavigation.PreviousUrl(effectivePage, content.HasPrevious),
                CatalogNavigation.NextUrl(effectivePage, content.HasNext),
                $"Página {effectivePage.ToString(Spanish)} de {totalPages.ToString(Spanish)}",
                state,
                PropertyPresentation.MessageFor(state),
                CatalogNavigation.RetryCatalogUrl(effectivePage));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception) when (IsExpectedFailure(exception))
        {
            return ErrorCatalog(requestedPage);
        }
    }

    public async Task<PropertyDetailsViewModel> GetDetailsAsync(Guid id, int page, CancellationToken cancellationToken)
    {
        var sourcePage = NormalizePage(page);

        try
        {
            using var response = await _api.GetByIdAsync(id, cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return DetailsPlaceholder(id, sourcePage, PresentationState.NoEncontrado);
            }

            if (!response.IsSuccessStatusCode || response.Content is null)
            {
                return DetailsPlaceholder(id, sourcePage, PresentationState.Error);
            }

            var item = response.Content;
            return new PropertyDetailsViewModel(
                item.Id,
                item.Title,
                item.Description,
                item.Address,
                FormatPrice(item.Price),
                PropertyStatusLabels.Resolve(item.Status),
                item.BedroomCount,
                item.BathroomCount,
                item.AreaSquareMeters,
                NormalizeImageUrl(item.ImageUrl),
                BuildImageAlt(item.Title, item.ImageUrl),
                FormatDate(item.CreatedAt),
                item.UpdatedAt.HasValue ? FormatDate(item.UpdatedAt.Value) : "No disponible",
                CatalogNavigation.BackUrl(sourcePage),
                PresentationState.Contenido,
                PropertyPresentation.MessageFor(PresentationState.Contenido, true),
                null);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception) when (IsExpectedFailure(exception))
        {
            return DetailsPlaceholder(id, sourcePage, PresentationState.Error);
        }
    }

    private static PropertyCardViewModel MapCard(ListPropertiesItemResponse item, int page)
    {
        var imageUrl = NormalizeImageUrl(item.ImageUrl);

        return new PropertyCardViewModel(
            item.Id,
            item.Title,
            Truncate(item.Description),
            item.Address,
            FormatPrice(item.Price),
            PropertyStatusLabels.Resolve(item.Status),
            item.BedroomCount,
            item.BathroomCount,
            item.AreaSquareMeters,
            imageUrl,
            BuildImageAlt(item.Title, item.ImageUrl),
            CatalogNavigation.DetailsUrl(item.Id, page));
    }

    private static PropertyCatalogViewModel ErrorCatalog(int page)
    {
        var state = PresentationState.Error;

        return new PropertyCatalogViewModel(
            [],
            page,
            CatalogNavigation.PageSize,
            0,
            1,
            false,
            false,
            null,
            null,
            $"Página {page.ToString(Spanish)} de 1",
            state,
            PropertyPresentation.MessageFor(state),
            CatalogNavigation.RetryCatalogUrl(page));
    }

    private static PropertyDetailsViewModel DetailsPlaceholder(Guid id, int page, PresentationState state)
    {
        var retryUrl = state == PresentationState.NoEncontrado
            ? CatalogNavigation.BackUrl(page)
            : CatalogNavigation.RetryDetailsUrl(id, page);

        return new PropertyDetailsViewModel(
            id,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            0,
            0,
            0,
            null,
            string.Empty,
            string.Empty,
            string.Empty,
            CatalogNavigation.BackUrl(page),
            state,
            PropertyPresentation.MessageFor(state, true),
            retryUrl);
    }

    private static string Truncate(string value) =>
        string.IsNullOrEmpty(value) || value.Length <= 180 ? value : $"{value[..180]}…";

    private static string FormatPrice(decimal value) => string.Format(Spanish, "{0:N2}", value);

    private static string FormatDate(DateTimeOffset value) => value.ToString("dd/MM/yyyy HH:mm", Spanish);

    private static string? NormalizeImageUrl(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string BuildImageAlt(string title, string? imageUrl) =>
        NormalizeImageUrl(imageUrl) is null
            ? $"Imagen no disponible para {title}"
            : $"Imagen de {title}";

    private static int NormalizePage(int page) => page < 1 ? 1 : page;

    private static bool IsExpectedFailure(Exception exception) =>
        exception is HttpRequestException
            or OperationCanceledException
            or ApiException
            or JsonException;
}
