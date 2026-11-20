using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace RealtorApi.Infrastructure.OpenApi;

public sealed class OpenApiSchemaCatalog : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        EnsureSchemas(document);

        if (document.Components?.Schemas is not null)
        {
            foreach (var schema in document.Components.Schemas.Values.ToList())
            {
                RewritePropertyStatusReference(schema, document, new HashSet<IOpenApiSchema>());
            }
        }

        if (document.Paths is not null)
        {
            foreach (var pathItem in document.Paths.Values)
            {
                if (pathItem.Operations is null)
                {
                    continue;
                }

                foreach (var operation in pathItem.Operations.Values)
                {
                    if (operation.Responses is not null)
                    {
                        foreach (var response in operation.Responses)
                        {
                            var isError = response.Key.StartsWith("4", StringComparison.Ordinal)
                                || response.Key.StartsWith("5", StringComparison.Ordinal);

                            if (!isError)
                            {
                                continue;
                            }

                            var schemaName = string.Equals(response.Key, "400", StringComparison.Ordinal)
                                ? RealtorOpenApiConstants.ValidationProblemDetailsSchema
                                : RealtorOpenApiConstants.ProblemDetailsSchema;

                            RewriteContent(response.Value.Content?.Values, document, schemaName);
                        }
                    }

                    if (operation.RequestBody is not null)
                    {
                        RewriteContent(operation.RequestBody.Content?.Values, document, null);
                    }
                }
            }
        }

        return Task.CompletedTask;
    }

    private static void EnsureSchemas(OpenApiDocument document)
    {
        var components = document.Components ??= new OpenApiComponents();
        var schemas = components.Schemas ??= new Dictionary<string, IOpenApiSchema>();

        schemas[RealtorOpenApiConstants.PropertyStatusSchema] = new OpenApiSchema
        {
            Type = JsonSchemaType.String,
            Description = "Available, Rented, Maintenance"
        };

        schemas[RealtorOpenApiConstants.ProblemDetailsSchema] = CreateProblemDetailsSchema();

        schemas[RealtorOpenApiConstants.ValidationProblemDetailsSchema] = new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            Properties = new Dictionary<string, IOpenApiSchema>
            {
                ["type"] = StringSchema(),
                ["title"] = StringSchema(),
                ["status"] = IntegerSchema(),
                ["detail"] = StringSchema(),
                ["instance"] = StringSchema(),
                ["errors"] = new OpenApiSchema
                {
                    Type = JsonSchemaType.Object
                }
            }
        };
    }

    private static OpenApiSchema CreateProblemDetailsSchema() => new()
    {
        Type = JsonSchemaType.Object,
        Properties = new Dictionary<string, IOpenApiSchema>
        {
            ["type"] = StringSchema(),
            ["title"] = StringSchema(),
            ["status"] = IntegerSchema(),
            ["detail"] = StringSchema(),
            ["instance"] = StringSchema()
        }
    };

    private static OpenApiSchema StringSchema() => new()
    {
        Type = JsonSchemaType.String
    };

    private static OpenApiSchema IntegerSchema() => new()
    {
        Type = JsonSchemaType.Integer,
        Format = "int32"
    };

    private static void RewriteContent(
        IEnumerable<IOpenApiMediaType>? content,
        OpenApiDocument document,
        string? errorSchemaName)
    {
        foreach (var mediaType in content ?? Enumerable.Empty<IOpenApiMediaType>())
        {
            if (mediaType is not OpenApiMediaType concreteMediaType)
            {
                continue;
            }

            if (errorSchemaName is not null)
            {
                concreteMediaType.Schema = new OpenApiSchemaReference(errorSchemaName, document);
                continue;
            }

            RewritePropertyStatusReference(concreteMediaType.Schema, document, new HashSet<IOpenApiSchema>());
        }
    }

    private static void RewritePropertyStatusReference(
        IOpenApiSchema? schema,
        OpenApiDocument document,
        HashSet<IOpenApiSchema> visited)
    {
        if (schema is not OpenApiSchema concreteSchema || !visited.Add(concreteSchema))
        {
            return;
        }

        if (concreteSchema.Properties is not null &&
            concreteSchema.Properties.TryGetValue("status", out var statusSchema) &&
            statusSchema is OpenApiSchema { Type: JsonSchemaType.String })
        {
            concreteSchema.Properties["status"] = new OpenApiSchemaReference(
                RealtorOpenApiConstants.PropertyStatusSchema,
                document);
        }

        if (concreteSchema.Items is not null)
        {
            RewritePropertyStatusReference(concreteSchema.Items, document, visited);
        }

        if (concreteSchema.Properties is not null)
        {
            foreach (var propertySchema in concreteSchema.Properties.Values.ToList())
            {
                RewritePropertyStatusReference(propertySchema, document, visited);
            }
        }
    }
}
