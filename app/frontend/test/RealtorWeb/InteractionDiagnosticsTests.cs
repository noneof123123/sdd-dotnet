using RealtorWeb.Features.Interactions;

namespace RealtorWebTests;

public class InteractionDiagnosticsTests
{
    [Fact]
    public void ConsultaLentaSuperaUmbralDeUnSegundo()
    {
        Assert.False(InteractionDiagnostics.EsConsultaLenta(TimeSpan.FromMilliseconds(999)));
        Assert.False(InteractionDiagnostics.EsConsultaLenta(TimeSpan.FromSeconds(1)));
        Assert.True(InteractionDiagnostics.EsConsultaLenta(TimeSpan.FromMilliseconds(1001)));
    }

    [Fact]
    public void ReconexionHastaDiezSegundosSeClasificaComoRecuperada()
    {
        Assert.True(InteractionDiagnostics.EsRecuperacionBreve(TimeSpan.Zero));
        Assert.True(InteractionDiagnostics.EsRecuperacionBreve(TimeSpan.FromSeconds(10)));
        Assert.False(InteractionDiagnostics.EsRecuperacionBreve(TimeSpan.FromMilliseconds(10001)));
        Assert.Equal(IncidentKind.Recuperado, InteractionDiagnostics.ClasificarReconexion(TimeSpan.FromSeconds(10)));
        Assert.Equal(IncidentKind.NoRecuperado, InteractionDiagnostics.ClasificarReconexion(TimeSpan.FromSeconds(11)));
    }

    [Theory]
    [InlineData(IncidentKind.Lento, "Consulta lenta finalizada")]
    [InlineData(IncidentKind.NoDisponible, "Contenido no disponible")]
    [InlineData(IncidentKind.Recuperado, "Conexión restablecida")]
    [InlineData(IncidentKind.NoRecuperado, "Conexión no restablecida")]
    public void ResultadoEstaControladoPorTipo(IncidentKind tipo, string esperado)
    {
        Assert.Equal(esperado, InteractionDiagnostics.ResultadoPara(tipo));
    }

    [Fact]
    public void IncidenteSoloContieneCamposOperativosPermitidos()
    {
        var momento = DateTimeOffset.Parse("2026-01-02T03:04:05.0000000+00:00");
        var incidente = InteractionDiagnostics.CrearIncidente(
            IncidentKind.NoRecuperado,
            InteractiveView.Detalle,
            momento);

        Assert.Equal(new IncidentRecord(
            IncidentKind.NoRecuperado,
            momento,
            InteractiveView.Detalle,
            "Conexión no restablecida"), incidente);
    }

    [Fact]
    public void ContextosDeVisitaDistintosNoCompartenEstado()
    {
        using var catalogo = new InteractionContext();
        using var detalle = new InteractionContext();

        catalogo.RegistrarCatalogo("3", "/?page=3");
        detalle.RegistrarDetalle(Guid.NewGuid(), 7, "/?page=7");
        catalogo.CerrarVisita();

        Assert.Equal(InteractiveView.Catalogo, catalogo.VistaActual);
        Assert.Equal(InteractiveView.Detalle, detalle.VistaActual);
        Assert.Equal(3, catalogo.PaginaSaneada);
        Assert.Equal(7, detalle.PaginaSaneada);
        Assert.NotNull(catalogo.Fin);
        Assert.Null(detalle.Fin);
    }
}
