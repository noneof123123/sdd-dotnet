using RealtorWeb.Features.Properties;

namespace RealtorWebTests;

public class PropertyStatusLabelsTests
{
    [Theory]
    [InlineData(PropertyStatus.Available, "Disponible")]
    [InlineData(PropertyStatus.Rented, "Alquilada")]
    [InlineData(PropertyStatus.Maintenance, "Mantenimiento")]
    [InlineData("Desconocido", "Estado no disponible")]
    [InlineData(null, "Estado no disponible")]
    public void ResolveReturnsSpanishLabel(string? status, string expected)
    {
        Assert.Equal(expected, PropertyStatusLabels.Resolve(status));
    }

    [Fact]
    public void ResolveNeverReturnsTechnicalStatus()
    {
        var result = PropertyStatusLabels.Resolve(PropertyStatus.Available);

        Assert.DoesNotContain(PropertyStatus.Available, result);
        Assert.Equal(PropertyStatusLabels.Disponible, result);
    }
}
