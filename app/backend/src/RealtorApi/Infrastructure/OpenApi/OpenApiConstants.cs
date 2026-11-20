namespace RealtorApi.Infrastructure.OpenApi;

public static class RealtorOpenApiConstants
{
    public const string DocumentName = "v1";
    public const string InfoTitle = "RealtorApi";
    public const string InfoVersion = "v1";
    public const string PublicRoute = "/openapi/v1.json";
    public const string HealthTag = "Salud";
    public const string PropertiesTag = "Propiedades";
    public const string GetHealthOperationId = "GetHealth";
    public const string GetPropertiesOperationId = "GetProperties";
    public const string GetPropertyByIdOperationId = "GetPropertyById";
    public const string CreatePropertyOperationId = "CreateProperty";
    public const string UpdatePropertyOperationId = "UpdateProperty";
    public const string ChangePropertyStatusOperationId = "ChangePropertyStatus";
    public const string PropertyStatusSchema = "PropertyStatus";
    public const string ProblemDetailsSchema = "ProblemDetails";
    public const string ValidationProblemDetailsSchema = "HttpValidationProblemDetails";
}
