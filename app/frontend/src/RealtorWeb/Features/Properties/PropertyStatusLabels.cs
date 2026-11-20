namespace RealtorWeb.Features.Properties;

public static class PropertyStatusLabels
{
    public const string Disponible = "Disponible";
    public const string Alquilada = "Alquilada";
    public const string Mantenimiento = "Mantenimiento";
    public const string NoDisponible = "Estado no disponible";

    public static string Resolve(string? status) => status switch
    {
        PropertyStatus.Available => Disponible,
        PropertyStatus.Rented => Alquilada,
        PropertyStatus.Maintenance => Mantenimiento,
        _ => NoDisponible
    };
}
