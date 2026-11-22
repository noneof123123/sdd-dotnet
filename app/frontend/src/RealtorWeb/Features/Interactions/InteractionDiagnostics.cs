namespace RealtorWeb.Features.Interactions;

public sealed record IncidentRecord(
    IncidentKind Tipo,
    DateTimeOffset Momento,
    InteractiveView Vista,
    string Resultado);

public static class InteractionDiagnostics
{
    public static readonly TimeSpan UmbralConsultaLenta = TimeSpan.FromSeconds(1);
    public static readonly TimeSpan UmbralReconexion = TimeSpan.FromSeconds(10);

    private static readonly IReadOnlyDictionary<IncidentKind, string> Resultados =
        new Dictionary<IncidentKind, string>
        {
            [IncidentKind.Lento] = "Consulta lenta finalizada",
            [IncidentKind.NoDisponible] = "Contenido no disponible",
            [IncidentKind.Recuperado] = "Conexión restablecida",
            [IncidentKind.NoRecuperado] = "Conexión no restablecida"
        };

    public static bool EsConsultaLenta(TimeSpan duracion) => duracion > UmbralConsultaLenta;

    public static bool EsRecuperacionBreve(TimeSpan duracion) => duracion <= UmbralReconexion;

    public static IncidentKind ClasificarReconexion(TimeSpan duracion) =>
        EsRecuperacionBreve(duracion) ? IncidentKind.Recuperado : IncidentKind.NoRecuperado;

    public static string ResultadoPara(IncidentKind tipo) => Resultados[tipo];

    public static IncidentRecord CrearIncidente(
        IncidentKind tipo,
        InteractiveView vista,
        DateTimeOffset momento) => new(tipo, momento, vista, ResultadoPara(tipo));
}
