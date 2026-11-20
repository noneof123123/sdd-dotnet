using Microsoft.Extensions.Logging;

namespace RealtorApi.Infrastructure.Logging;

public static partial class InfrastructureLog
{
    [LoggerMessage(EventId = 1001, Level = LogLevel.Information, Message = "Infraestructura iniciada: {Operation}")]
    public static partial void InfrastructureStarted(ILogger logger, string operation);

    [LoggerMessage(EventId = 1002, Level = LogLevel.Warning, Message = "Operación rechazada: {Operation}")]
    public static partial void OperationRejected(ILogger logger, string operation);

    [LoggerMessage(EventId = 1003, Level = LogLevel.Information, Message = "Propiedad consultada: {PropertyId}")]
    public static partial void PropertyQueried(ILogger logger, Guid propertyId);

    [LoggerMessage(EventId = 1004, Level = LogLevel.Information, Message = "Estado de propiedad cambiado: {PropertyId} -> {Status}")]
    public static partial void PropertyStatusChanged(ILogger logger, Guid propertyId, string status);
}
