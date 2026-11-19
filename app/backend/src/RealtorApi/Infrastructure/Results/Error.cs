namespace RealtorApi.Infrastructure.Results;

public sealed record Error(string Code, ErrorType Type, string Title, string Detail)
{
    public static Error Validation(string code, string detail) =>
        new(code, ErrorType.Validation, "Solicitud inválida", detail);

    public static Error NotFound(string code, string detail) =>
        new(code, ErrorType.NotFound, "Recurso no encontrado", detail);

    public static Error Conflict(string code, string detail) =>
        new(code, ErrorType.Conflict, "Conflicto", detail);
}

public enum ErrorType
{
    Validation,
    NotFound,
    Conflict
}
