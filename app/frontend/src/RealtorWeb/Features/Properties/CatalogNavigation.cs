using System.Globalization;

namespace RealtorWeb.Features.Properties;

public static class CatalogNavigation
{
    public const int PageSize = 12;

    public static int SanitizePage(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 1;
        }

        return int.TryParse(value.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var page) && page > 0
            ? page
            : 1;
    }

    public static string CatalogUrl(int page) => $"/?page={Normalize(page).ToString(CultureInfo.InvariantCulture)}";

    public static string DetailsUrl(Guid id, int page) =>
        $"/properties/{id.ToString("D", CultureInfo.InvariantCulture)}?page={Normalize(page).ToString(CultureInfo.InvariantCulture)}";

    public static string? PreviousUrl(int page, bool hasPrevious) =>
        hasPrevious ? CatalogUrl(Normalize(page) - 1) : null;

    public static string? NextUrl(int page, bool hasNext) =>
        hasNext ? CatalogUrl(Normalize(page) + 1) : null;

    public static string RetryCatalogUrl(int page) => CatalogUrl(page);

    public static string RetryDetailsUrl(Guid id, int page) => DetailsUrl(id, page);

    public static string BackUrl(int page) => CatalogUrl(page);

    private static int Normalize(int page) => page < 1 ? 1 : page;
}
