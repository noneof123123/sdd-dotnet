using RealtorWeb.Features.Properties;

namespace RealtorWebTests;

public class CatalogNavigationTests
{
    [Theory]
    [InlineData(null, 1)]
    [InlineData("", 1)]
    [InlineData("   ", 1)]
    [InlineData("no-valido", 1)]
    [InlineData("0", 1)]
    [InlineData("-1", 1)]
    [InlineData("+2", 1)]
    [InlineData("999999999999999999999", 1)]
    [InlineData("7", 7)]
    public void SanitizePageReturnsOnlyPositiveIntegers(string? value, int expected)
    {
        Assert.Equal(expected, CatalogNavigation.SanitizePage(value));
    }

    [Fact]
    public void CatalogUrlUsesSanitizedPage()
    {
        Assert.Equal("/?page=1", CatalogNavigation.CatalogUrl(0));
        Assert.Equal("/?page=4", CatalogNavigation.CatalogUrl(4));
    }

    [Fact]
    public void DetailsUrlPreservesSanitizedPage()
    {
        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");

        Assert.Equal(
            "/properties/11111111-1111-1111-1111-111111111111?page=2",
            CatalogNavigation.DetailsUrl(id, 2));
        Assert.Equal(
            "/properties/11111111-1111-1111-1111-111111111111?page=1",
            CatalogNavigation.DetailsUrl(id, -3));
    }

    [Fact]
    public void PaginationLinksAppearOnlyWhenAllowed()
    {
        Assert.Null(CatalogNavigation.PreviousUrl(1, false));
        Assert.Null(CatalogNavigation.NextUrl(3, false));
        Assert.Equal("/?page=1", CatalogNavigation.PreviousUrl(2, true));
        Assert.Equal("/?page=4", CatalogNavigation.NextUrl(3, true));
    }

    [Fact]
    public void RetryAndBackLinksUseSanitizedPage()
    {
        Assert.Equal("/?page=2", CatalogNavigation.RetryCatalogUrl(2));
        Assert.Equal(
            "/properties/11111111-1111-1111-1111-111111111111?page=2",
            CatalogNavigation.RetryDetailsUrl(Guid.Parse("11111111-1111-1111-1111-111111111111"), 2));
        Assert.Equal("/?page=2", CatalogNavigation.BackUrl(2));
    }
}
