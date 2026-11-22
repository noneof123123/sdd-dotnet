using RealtorWeb.Features.Properties;

namespace RealtorWeb.Features.Interactions;

public sealed class InteractionContext : IDisposable
{
    private readonly object _sync = new();

    public DateTimeOffset Inicio { get; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? Fin { get; private set; }

    public InteractiveView VistaActual { get; private set; } = InteractiveView.Catalogo;

    public InteractionOutcome UltimoResultado { get; private set; } = InteractionOutcome.Correcto;

    public InteractionStatus Estado { get; private set; } = InteractionStatus.Activa;

    public string? PaginaSolicitada { get; private set; }

    public int PaginaSaneada { get; private set; } = 1;

    public Guid? PropiedadId { get; private set; }

    public string DireccionRetorno { get; private set; } = "/";

    public void RegistrarCatalogo(string? paginaSolicitada, string direccionRetorno)
    {
        lock (_sync)
        {
            VistaActual = InteractiveView.Catalogo;
            PaginaSolicitada = paginaSolicitada;
            PaginaSaneada = CatalogNavigation.SanitizePage(paginaSolicitada);
            PropiedadId = null;
            DireccionRetorno = NormalizarRetorno(direccionRetorno);
        }
    }

    public void RegistrarDetalle(Guid id, int page, string direccionRetorno)
    {
        lock (_sync)
        {
            VistaActual = InteractiveView.Detalle;
            PaginaSolicitada = page.ToString(System.Globalization.CultureInfo.InvariantCulture);
            PaginaSaneada = CatalogNavigation.SanitizePage(PaginaSolicitada);
            PropiedadId = id;
            DireccionRetorno = NormalizarRetorno(direccionRetorno);
        }
    }

    public void ComenzarEspera()
    {
        lock (_sync)
        {
            Estado = InteractionStatus.Esperando;
        }
    }

    public void CompletarCorrecto(bool lento = false)
    {
        lock (_sync)
        {
            Estado = InteractionStatus.Activa;
            UltimoResultado = lento ? InteractionOutcome.Lento : InteractionOutcome.Correcto;
        }
    }

    public void CompletarNoDisponible()
    {
        lock (_sync)
        {
            Estado = InteractionStatus.NoDisponible;
            UltimoResultado = InteractionOutcome.NoDisponible;
        }
    }

    public void ComenzarRecuperacion()
    {
        lock (_sync)
        {
            if (Estado != InteractionStatus.NoDisponible)
            {
                Estado = InteractionStatus.Recuperando;
            }
        }
    }

    public void FinalizarRecuperacion(bool recuperada)
    {
        lock (_sync)
        {
            Estado = recuperada ? InteractionStatus.Activa : InteractionStatus.NoDisponible;
            UltimoResultado = recuperada ? InteractionOutcome.Recuperado : InteractionOutcome.NoDisponible;
        }
    }

    public void CerrarVisita()
    {
        lock (_sync)
        {
            Fin ??= DateTimeOffset.UtcNow;
        }
    }

    public void Dispose()
    {
        CerrarVisita();
    }

    private static string NormalizarRetorno(string? direccion)
    {
        if (string.IsNullOrWhiteSpace(direccion))
        {
            return "/";
        }

        var valor = direccion.Trim();
        return valor.StartsWith('/') && !valor.StartsWith("//") ? valor : "/";
    }
}
