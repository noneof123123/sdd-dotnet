using Microsoft.Extensions.Logging;
using RealtorWeb.Features.Interactions;
using RealtorWeb.Features.Properties;

namespace RealtorWebTests;

public class InteractionLoggingCatalogServiceTests
{
    [Fact]
    public async Task GetCatalogAsyncRegistraConsultaLentaConResultadoControlado()
    {
        var logger = new ListLogger<InteractionLoggingCatalogService>();
        var service = CrearServicio(logger, TimeSpan.FromMilliseconds(1), async (_, _) =>
        {
            await Task.Delay(10);
            return CatalogTestData.Catalog();
        });

        await service.GetCatalogAsync(1, CancellationToken.None);

        Assert.Contains("Tipo: Lento", Assert.Single(logger.Entradas));
        Assert.Contains("Vista: Catalogo", logger.Entradas[0]);
        Assert.Contains("Resultado: Consulta lenta finalizada", logger.Entradas[0]);
    }

    [Fact]
    public async Task GetCatalogAsyncRegistraContenidoNoDisponible()
    {
        var logger = new ListLogger<InteractionLoggingCatalogService>();
        var service = CrearServicio(logger, TimeSpan.FromSeconds(10), (_, _) =>
            Task.FromResult(CatalogTestData.CatalogWith(
                state: PresentationState.Error,
                message: PropertyPresentation.MessageFor(PresentationState.Error),
                retryUrl: CatalogNavigation.RetryCatalogUrl(1))));

        await service.GetCatalogAsync(1, CancellationToken.None);

        Assert.Contains("Tipo: NoDisponible", Assert.Single(logger.Entradas));
        Assert.Contains("Resultado: Contenido no disponible", logger.Entradas[0]);
    }

    [Fact]
    public async Task GetDetailsAsyncRegistraErrorYVistaDetalle()
    {
        var logger = new ListLogger<InteractionLoggingCatalogService>();
        var service = CrearServicio(logger, TimeSpan.FromSeconds(10), detailsHandler: (_, _, _) =>
            throw new InvalidOperationException("fallo de prueba"));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.GetDetailsAsync(Guid.NewGuid(), 1, CancellationToken.None));

        Assert.Contains("Tipo: NoDisponible", Assert.Single(logger.Entradas));
        Assert.Contains("Vista: Detalle", logger.Entradas[0]);
        Assert.Contains("Resultado: Contenido no disponible", logger.Entradas[0]);
    }

    [Fact]
    public async Task RegistroContieneSoloLosCamposPermitidos()
    {
        var momento = DateTimeOffset.Parse("2026-01-02T03:04:05.0000000+00:00");
        var logger = new ListLogger<InteractionLoggingCatalogService>();
        var service = new InteractionLoggingCatalogService(
            new StubCatalogService(),
            new InteractionContext(),
            logger,
            new FixedTimeProvider(momento),
            TimeSpan.FromTicks(-1));

        await service.GetCatalogAsync(1, CancellationToken.None);

        Assert.Equal(
            "Incidente de interacción. Tipo: Lento, Momento: 2026-01-02T03:04:05.0000000+00:00, Vista: Catalogo, Resultado: Consulta lenta finalizada",
            Assert.Single(logger.Entradas));
    }

    private static InteractionLoggingCatalogService CrearServicio(
        ListLogger<InteractionLoggingCatalogService> logger,
        TimeSpan umbral,
        Func<int, CancellationToken, Task<PropertyCatalogViewModel>>? catalogHandler = null,
        Func<Guid, int, CancellationToken, Task<PropertyDetailsViewModel>>? detailsHandler = null)
    {
        var inner = new StubCatalogService();
        if (catalogHandler is not null)
        {
            inner.CatalogHandler = catalogHandler;
        }

        if (detailsHandler is not null)
        {
            inner.DetailsHandler = detailsHandler;
        }

        return new InteractionLoggingCatalogService(
            inner,
            new InteractionContext(),
            logger,
            TimeProvider.System,
            umbral);
    }

    private sealed class ListLogger<T> : ILogger<T>
    {
        public List<string> Entradas { get; } = [];

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter) =>
            Entradas.Add(formatter(state, exception));
    }

    private sealed class FixedTimeProvider(DateTimeOffset momento) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => momento;
    }
}
