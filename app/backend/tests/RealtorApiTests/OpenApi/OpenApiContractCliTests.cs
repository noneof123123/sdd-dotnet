using System.Text.Json;

namespace RealtorApiTests.OpenApi;

[Collection("OpenApi")]
public sealed class OpenApiContractCliTests
{
    [Fact]
    public void GeneratedDocument_ExposesChangePropertyStatusContract()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(OpenApiTestApp.DocumentPath));
        var operation = document.RootElement
            .GetProperty("paths")
            .GetProperty("/api/properties/{id}/status")
            .GetProperty("patch");

        Assert.Equal("ChangePropertyStatus", operation.GetProperty("operationId").GetString());
        Assert.Equal("Propiedades", operation.GetProperty("tags")[0].GetString());

        var pathParameter = Assert.Single(operation.GetProperty("parameters").EnumerateArray());
        Assert.Equal("id", pathParameter.GetProperty("name").GetString());
        Assert.Equal("path", pathParameter.GetProperty("in").GetString());
        Assert.True(pathParameter.GetProperty("required").GetBoolean());

        Assert.Equal(
            "#/components/schemas/ChangePropertyStatusBody",
            operation.GetProperty("requestBody").GetProperty("content").GetProperty("application/json").GetProperty("schema").GetProperty("$ref").GetString());

        AssertRef(operation, "200", "application/json", "#/components/schemas/ChangePropertyStatusResponse");
        AssertRef(operation, "400", "application/problem+json", "#/components/schemas/HttpValidationProblemDetails");
        AssertRef(operation, "404", "application/problem+json", "#/components/schemas/ProblemDetails");
        AssertRef(operation, "500", "application/problem+json", "#/components/schemas/ProblemDetails");
    }

    private static void AssertRef(JsonElement operation, string statusCode, string contentType, string expectedRef)
    {
        var schema = operation.GetProperty("responses").GetProperty(statusCode).GetProperty("content").GetProperty(contentType).GetProperty("schema");
        Assert.Equal(expectedRef, schema.GetProperty("$ref").GetString());
    }
}
