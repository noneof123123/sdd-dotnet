namespace RealtorWeb.Features.Interactions;

public enum InteractiveView
{
    Catalogo,
    Detalle
}

public enum InteractionOutcome
{
    Correcto,
    Lento,
    NoDisponible,
    Recuperado
}

public enum InteractionStatus
{
    Activa,
    Esperando,
    Recuperando,
    NoDisponible
}

public enum IncidentKind
{
    Lento,
    NoDisponible,
    Recuperado,
    NoRecuperado
}
