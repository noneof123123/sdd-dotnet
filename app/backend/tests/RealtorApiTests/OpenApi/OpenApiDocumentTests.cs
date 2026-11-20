using System.Text.Json;

namespace RealtorApiTests.OpenApi;

[Collection("OpenApi")]
public sealed class OpenApiDocumentTests
{
    [Fact]
    public void GeneratedDocument_MatchesStructuralContract()
    {
        var json = File.ReadAllText(OpenApiTestApp.DocumentPath);
        using var parsed = JsonDocument.Parse(json);
        var root = parsed.RootElement;

        Assert.StartsWith("3.0.", root.GetProperty("openapi").GetString());
        Assert.Equal("v1", root.GetProperty("info").GetProperty("version").GetString());

        var paths = root.GetProperty("paths");
        Assert.Equal(4, paths.EnumerateObject().Count());
        Assert.Equal(6, paths.EnumerateObject().Sum(path => path.Value.EnumerateObject().Count()));
        Assert.False(paths.TryGetProperty("/openapi/v1.json", out _));
        Assert.DoesNotContain(":guid", json);

        AssertOperation(paths, "/health", "get", "GetHealth", "Salud");
        AssertOperation(paths, "/api/properties", "get", "GetProperties", "Propiedades");
        AssertOperation(paths, "/api/properties/{id}", "get", "GetPropertyById", "Propiedades");
        AssertOperation(paths, "/api/properties", "post", "CreateProperty", "Propiedades");
        AssertOperation(paths, "/api/properties/{id}", "put", "UpdateProperty", "Propiedades");
        AssertOperation(paths, "/api/properties/{id}/status", "patch", "ChangePropertyStatus", "Propiedades");

        var listOperation = paths.GetProperty("/api/properties").GetProperty("get");
        var queryNames = listOperation.GetProperty("parameters").EnumerateArray()
            .Where(parameter => parameter.GetProperty("in").GetString() == "query")
            .Select(parameter => parameter.GetProperty("name").GetString())
            .ToArray();
        Assert.Contains("page", queryNames);
        Assert.Contains("pageSize", queryNames);

        var healthOperation = paths.GetProperty("/health").GetProperty("get");
        AssertErrorRef(healthOperation, "500", "#/components/schemas/ProblemDetails");

        var changeStatusOperation = paths.GetProperty("/api/properties/{id}/status").GetProperty("patch");
        var pathParameter = Assert.Single(changeStatusOperation.GetProperty("parameters").EnumerateArray());
        Assert.Equal("id", pathParameter.GetProperty("name").GetString());
        Assert.Equal("path", pathParameter.GetProperty("in").GetString());
        Assert.True(pathParameter.GetProperty("required").GetBoolean());
        Assert.Equal(
            "#/components/schemas/ChangePropertyStatusBody",
            changeStatusOperation.GetProperty("requestBody").GetProperty("content").GetProperty("application/json").GetProperty("schema").GetProperty("$ref").GetString());

        var schemas = root.GetProperty("components").GetProperty("schemas");
        Assert.True(schemas.TryGetProperty("PropertyStatus", out _));
        Assert.True(schemas.TryGetProperty("ProblemDetails", out _));
        Assert.True(schemas.TryGetProperty("HttpValidationProblemDetails", out _));

        AssertStatusRef(schemas, "HealthResponse");
        AssertStatusRef(schemas, "ListPropertiesItemResponse");
        AssertStatusRef(schemas, "GetPropertyByIdResponse");
        AssertStatusRef(schemas, "CreatePropertyResponse");
        AssertStatusRef(schemas, "UpdatePropertyResponse");
        AssertStatusRef(schemas, "ChangePropertyStatusResponse");

        AssertErrorRef(paths.GetProperty("/api/properties").GetProperty("get"), "500", "#/components/schemas/ProblemDetails");
        AssertErrorRef(paths.GetProperty("/api/properties/{id}").GetProperty("get"), "404", "#/components/schemas/ProblemDetails");
        AssertErrorRef(paths.GetProperty("/api/properties").GetProperty("post"), "400", "#/components/schemas/HttpValidationProblemDetails");
        AssertErrorRef(paths.GetProperty("/api/properties").GetProperty("post"), "409", "#/components/schemas/ProblemDetails");
        AssertErrorRef(paths.GetProperty("/api/properties/{id}").GetProperty("put"), "400", "#/components/schemas/HttpValidationProblemDetails");
        AssertErrorRef(paths.GetProperty("/api/properties/{id}").GetProperty("put"), "404", "#/components/schemas/ProblemDetails");
        AssertErrorRef(paths.GetProperty("/api/properties/{id}").GetProperty("put"), "409", "#/components/schemas/ProblemDetails");
        AssertErrorRef(changeStatusOperation, "400", "#/components/schemas/HttpValidationProblemDetails");
        AssertErrorRef(changeStatusOperation, "404", "#/components/schemas/ProblemDetails");
        AssertErrorRef(changeStatusOperation, "500", "#/components/schemas/ProblemDetails");
    }

    private static void AssertOperation(
        JsonElement paths,
        string path,
        string method,
        string operationId,
        string tag)
    {
        Assert.True(paths.TryGetProperty(path, out var pathItem));
        Assert.True(pathItem.TryGetProperty(method, out var operation));
        Assert.Equal(operationId, operation.GetProperty("operationId").GetString());
        Assert.Equal(tag, operation.GetProperty("tags")[0].GetString());
    }

    private static void AssertErrorRef(JsonElement operation, string statusCode, string expectedRef)
    {
        Assert.True(operation.GetProperty("responses").TryGetProperty(statusCode, out var response));
        var schema = response.GetProperty("content").GetProperty("application/problem+json").GetProperty("schema");
        Assert.Equal(expectedRef, schema.GetProperty("$ref").GetString());
    }

    private static void AssertStatusRef(JsonElement schemas, string schemaName)
    {
        var status = schemas.GetProperty(schemaName).GetProperty("properties").GetProperty("status");
        Assert.Equal("#/components/schemas/PropertyStatus", status.GetProperty("$ref").GetString());
    }
}
