using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.Logging;
using RealtorWeb.Features.Interactions;

namespace RealtorWebTests;

public class InteractionCircuitHandlerTests
{
    private static readonly DateTimeOffset Inicio = DateTimeOffset.Parse("2026-11-22T10:00:00.0000000+00:00");

    [Fact]
    public async Task InterrupcionBreveSeRecuperaSinRecarga()
    {
        using var context = new InteractionContext();
        var logger = new CaptureLogger();
        var tiempo = new TestTimeProvider { Ahora = Inicio };
        var handler = CrearHandler(context, logger, tiempo);

        await handler.OnConnectionDownAsync(null!, CancellationToken.None);
        Assert.Equal(InteractionStatus.Recuperando, context.Estado);

        tiempo.Ahora = Inicio.AddSeconds(10);
        await handler.OnConnectionUpAsync(null!, CancellationToken.None);

        Assert.Equal(InteractionStatus.Activa, context.Estado);
        Assert.Equal(InteractionOutcome.Recuperado, context.UltimoResultado);
        Assert.Contains("Conexión restablecida", Assert.Single(logger.Mensajes));
    }

    [Fact]
    public async Task InterrupcionProlongadaTerminaEnNoDisponible()
    {
        using var context = new InteractionContext();
        var logger = new CaptureLogger();
        var tiempo = new TestTimeProvider { Ahora = Inicio };
        var handler = CrearHandler(context, logger, tiempo);

        await handler.OnConnectionDownAsync(null!, CancellationToken.None);
        tiempo.Ahora = Inicio.AddMilliseconds(10001);
        await handler.OnConnectionUpAsync(null!, CancellationToken.None);

        Assert.Equal(InteractionStatus.NoDisponible, context.Estado);
        Assert.Equal(InteractionOutcome.NoDisponible, context.UltimoResultado);
        Assert.Contains("Conexión no restablecida", Assert.Single(logger.Mensajes));
    }

    [Fact]
    public async Task CierreDelCircuitoCierraVisitaYRegistraInterrupcionPendiente()
    {
        using var context = new InteractionContext();
        var logger = new CaptureLogger();
        var tiempo = new TestTimeProvider { Ahora = Inicio };
        var handler = CrearHandler(context, logger, tiempo);

        await handler.OnConnectionDownAsync(null!, CancellationToken.None);
        await handler.OnCircuitClosedAsync(null!, CancellationToken.None);

        Assert.NotNull(context.Fin);
        Assert.Contains("Conexión no restablecida", Assert.Single(logger.Mensajes));
    }

    [Fact]
    public async Task RecuperacionSinCaidaNoRegistraIncidente()
    {
        using var context = new InteractionContext();
        var logger = new CaptureLogger();
        var handler = CrearHandler(context, logger, new TestTimeProvider { Ahora = Inicio });

        await handler.OnConnectionUpAsync(null!, CancellationToken.None);

        Assert.Equal(InteractionStatus.Activa, context.Estado);
        Assert.Empty(logger.Mensajes);
    }

    private static InteractionCircuitHandler CrearHandler(
        InteractionContext context,
        ILogger<InteractionCircuitHandler> logger,
        TimeProvider tiempo)
        => new(context, logger, tiempo);

    private sealed class TestTimeProvider : TimeProvider
    {
        public DateTimeOffset Ahora { get; set; }

        public override DateTimeOffset GetUtcNow() => Ahora;
    }

    private sealed class CaptureLogger : ILogger<InteractionCircuitHandler>
    {
        public List<string> Mensajes { get; } = [];

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
            => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
            => Mensajes.Add(formatter(state, exception));
    }
}
