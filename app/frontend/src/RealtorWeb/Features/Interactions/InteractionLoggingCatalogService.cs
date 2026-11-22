using RealtorWeb.Features.Properties;

namespace RealtorWeb.Features.Interactions;

public sealed class InteractionLoggingCatalogService : IPropertyCatalogService
{
    private readonly IPropertyCatalogService _inner;
    private readonly InteractionContext _context;
    private readonly ILogger<InteractionLoggingCatalogService> _logger;
    private readonly TimeProvider _timeProvider;
    private readonly TimeSpan _umbralLento;

    public InteractionLoggingCatalogService(
        IPropertyCatalogService inner,
        InteractionContext context,
        ILogger<InteractionLoggingCatalogService> logger,
        TimeProvider? timeProvider = null,
        TimeSpan? umbralLento = null)
    {
        _inner = inner;
        _context = context;
        _logger = logger;
        _timeProvider = timeProvider ?? TimeProvider.System;
        _umbralLento = umbralLento ?? InteractionDiagnostics.UmbralConsultaLenta;
    }

    public async Task<PropertyCatalogViewModel> GetCatalogAsync(int page, CancellationToken cancellationToken)
    {
        var inicio = _timeProvider.GetTimestamp();
        try
        {
            var resultado = await _inner.GetCatalogAsync(page, cancellationToken);
            var duracion = _timeProvider.GetElapsedTime(inicio);
            var lento = duracion > _umbralLento;

            if (resultado.State == PresentationState.Error)
            {
                RegistrarIncidente(IncidentKind.NoDisponible, InteractiveView.Catalogo);
                return resultado;
            }

            RegistrarSiEsLento(lento, InteractiveView.Catalogo);
            return resultado;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            RegistrarIncidente(IncidentKind.NoDisponible, InteractiveView.Catalogo);
            throw;
        }
    }

    public async Task<PropertyDetailsViewModel> GetDetailsAsync(Guid id, int page, CancellationToken cancellationToken)
    {
        var inicio = _timeProvider.GetTimestamp();
        try
        {
            var resultado = await _inner.GetDetailsAsync(id, page, cancellationToken);
            var duracion = _timeProvider.GetElapsedTime(inicio);
            var lento = duracion > _umbralLento;

            if (resultado.State == PresentationState.Error)
            {
                RegistrarIncidente(IncidentKind.NoDisponible, InteractiveView.Detalle);
                return resultado;
            }

            RegistrarSiEsLento(lento, InteractiveView.Detalle);
            return resultado;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            RegistrarIncidente(IncidentKind.NoDisponible, InteractiveView.Detalle);
            throw;
        }
    }

    private void RegistrarSiEsLento(bool lento, InteractiveView vista)
    {
        if (lento)
        {
            RegistrarIncidente(IncidentKind.Lento, vista);
            _context.CompletarCorrecto(lento: true);
            return;
        }

        _context.CompletarCorrecto();
    }

    private void RegistrarIncidente(IncidentKind tipo, InteractiveView vista)
    {
        var incidente = InteractionDiagnostics.CrearIncidente(tipo, vista, _timeProvider.GetUtcNow());
        _logger.LogInformation(
            "Incidente de interacción. Tipo: {Tipo}, Momento: {Momento:o}, Vista: {Vista}, Resultado: {Resultado}",
            incidente.Tipo,
            incidente.Momento,
            incidente.Vista,
            incidente.Resultado);

        if (tipo == IncidentKind.NoDisponible)
        {
            _context.CompletarNoDisponible();
        }
    }
}
