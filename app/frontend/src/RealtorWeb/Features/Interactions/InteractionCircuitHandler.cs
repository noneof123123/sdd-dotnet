using Microsoft.AspNetCore.Components.Server.Circuits;

namespace RealtorWeb.Features.Interactions;

public sealed class InteractionCircuitHandler : CircuitHandler
{
    private readonly InteractionContext _context;
    private readonly ILogger<InteractionCircuitHandler> _logger;
    private readonly TimeProvider _timeProvider;
    private DateTimeOffset? _inicioDesconexion;

    public InteractionCircuitHandler(
        InteractionContext context,
        ILogger<InteractionCircuitHandler> logger,
        TimeProvider? timeProvider = null)
    {
        _context = context;
        _logger = logger;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public override Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _inicioDesconexion ??= _timeProvider.GetUtcNow();
        _context.ComenzarRecuperacion();
        return Task.CompletedTask;
    }

    public override Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        if (_inicioDesconexion is not { } inicio)
        {
            return Task.CompletedTask;
        }

        var duracion = _timeProvider.GetUtcNow() - inicio;
        var recuperada = InteractionDiagnostics.EsRecuperacionBreve(duracion);
        var tipo = InteractionDiagnostics.ClasificarReconexion(duracion);

        _context.FinalizarRecuperacion(recuperada);
        RegistrarIncidente(tipo, _timeProvider.GetUtcNow());
        _inicioDesconexion = null;

        return Task.CompletedTask;
    }

    public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        if (_inicioDesconexion is not null)
        {
            RegistrarIncidente(IncidentKind.NoRecuperado, _timeProvider.GetUtcNow());
            _inicioDesconexion = null;
        }

        _context.CerrarVisita();
        return Task.CompletedTask;
    }

    private void RegistrarIncidente(IncidentKind tipo, DateTimeOffset momento)
    {
        var incidente = InteractionDiagnostics.CrearIncidente(tipo, _context.VistaActual, momento);
        _logger.LogInformation(
            "Incidente de interacción. Tipo: {Tipo}, Momento: {Momento:o}, Vista: {Vista}, Resultado: {Resultado}",
            incidente.Tipo,
            incidente.Momento,
            incidente.Vista,
            incidente.Resultado);
    }
}
