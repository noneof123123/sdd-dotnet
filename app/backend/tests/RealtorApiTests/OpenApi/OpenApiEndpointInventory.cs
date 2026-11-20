using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace RealtorApiTests.OpenApi;

public static class OpenApiEndpointInventory
{
    private static readonly string[] DocumentMethods = ["get", "post", "put", "patch", "delete"];

    public static HashSet<EndpointIdentity> FromRuntime()
    {
        using var app = OpenApiTestApp.CreateEndpointInventoryApp();
        var dataSourcesProperty = typeof(WebApplication).GetProperty(
            "DataSources",
            BindingFlags.Instance | BindingFlags.NonPublic);
        var dataSources = (IEnumerable<EndpointDataSource>)dataSourcesProperty!.GetValue(app)!;
        var identities = new HashSet<EndpointIdentity>();

        foreach (var endpoint in dataSources.SelectMany(source => source.Endpoints).OfType<RouteEndpoint>())
        {
            var route = endpoint.RoutePattern.RawText;
            var methods = endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods;

            if (route is null || methods is null)
            {
                continue;
            }

            foreach (var method in methods)
            {
                var identity = new EndpointIdentity(method.ToUpperInvariant(), NormalizeRoute(route));
                if (IsDocumentable(identity))
                {
                    identities.Add(identity);
                }
            }
        }

        return identities;
    }

    public static HashSet<EndpointIdentity> FromGeneratedDocument()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(OpenApiTestApp.DocumentPath));
        var identities = new HashSet<EndpointIdentity>();

        foreach (var pathProperty in document.RootElement.GetProperty("paths").EnumerateObject())
        {
            foreach (var methodProperty in pathProperty.Value.EnumerateObject())
            {
                if (!DocumentMethods.Contains(methodProperty.Name, StringComparer.OrdinalIgnoreCase))
                {
                    continue;
                }

                var identity = new EndpointIdentity(
                    methodProperty.Name.ToUpperInvariant(),
                    NormalizeRoute(pathProperty.Name));

                if (IsDocumentable(identity))
                {
                    identities.Add(identity);
                }
            }
        }

        return identities;
    }

    public static string NormalizeRoute(string route) =>
        Regex.Replace(route, @"\{([^}:]+):[^}]+\}", "{$1}");

    private static bool IsDocumentable(EndpointIdentity identity) =>
        identity.Route != "/openapi/v1.json" && identity.Route != "/test-slice";
}

public sealed record EndpointIdentity(string Method, string Route);
