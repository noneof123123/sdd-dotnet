using System.Text.RegularExpressions;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace RealtorApi.Infrastructure.OpenApi;

public sealed class OpenApiDocumentTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Info.Title = RealtorOpenApiConstants.InfoTitle;
        document.Info.Version = RealtorOpenApiConstants.InfoVersion;

        var stalePaths = document.Paths.Keys
            .Where(path => NormalizePath(path) == RealtorOpenApiConstants.PublicRoute)
            .ToList();

        foreach (var stalePath in stalePaths)
        {
            document.Paths.Remove(stalePath);
        }

        var normalizedPaths = document.Paths
            .Select(pair => new KeyValuePair<string, IOpenApiPathItem>(NormalizePath(pair.Key), pair.Value))
            .ToList();

        document.Paths.Clear();

        foreach (var normalizedPath in normalizedPaths)
        {
            document.Paths[normalizedPath.Key] = normalizedPath.Value;
        }

        return Task.CompletedTask;
    }

    internal static string NormalizePath(string path) =>
        Regex.Replace(path, @"\{([^}:]+):[^}]+\}", "{$1}");
}
