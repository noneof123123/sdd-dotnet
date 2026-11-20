namespace RealtorApiTests.OpenApi;

[Collection("OpenApi")]
public sealed class OpenApiDriftTests
{
    private static readonly EndpointIdentity Probe = new("GET", "/api/probe");

    [Fact]
    public void AlignedInventory_MatchesGeneratedDocument()
    {
        AssertEqual(OpenApiEndpointInventory.FromRuntime(), OpenApiEndpointInventory.FromGeneratedDocument());
    }

    [Fact]
    public void ChangeStatusEndpoint_IsPresentInBothInventories()
    {
        var expected = new EndpointIdentity("PATCH", "/api/properties/{id}/status");

        Assert.Contains(expected, OpenApiEndpointInventory.FromRuntime());
        Assert.Contains(expected, OpenApiEndpointInventory.FromGeneratedDocument());
    }

    [Fact]
    public void NewEndpoint_IsDetected()
    {
        var runtime = OpenApiEndpointInventory.FromRuntime();
        runtime.Add(Probe);

        AssertNotEqual(runtime, OpenApiEndpointInventory.FromGeneratedDocument());
    }

    [Fact]
    public void RenamedEndpoint_IsDetected()
    {
        var runtime = OpenApiEndpointInventory.FromRuntime();
        var renamed = new EndpointIdentity("GET", "/api/renamed");
        runtime.RemoveWhere(identity => identity == renamed);
        runtime.Add(renamed);

        AssertNotEqual(runtime, OpenApiEndpointInventory.FromGeneratedDocument());
    }

    [Fact]
    public void RemovedEndpoint_IsDetected()
    {
        var runtime = OpenApiEndpointInventory.FromRuntime();
        var removed = runtime.First(identity => identity.Route == "/api/properties/{id}" && identity.Method == "GET");
        runtime.Remove(removed);

        AssertNotEqual(runtime, OpenApiEndpointInventory.FromGeneratedDocument());
    }

    [Fact]
    public void RouteConstraint_AND_PlainRoute_AreEquivalent()
    {
        var constrained = new EndpointIdentity("GET", "/api/properties/{id:guid}");
        var normalized = new EndpointIdentity("GET", "/api/properties/{id}");

        Assert.Equal(normalized, new EndpointIdentity(
            constrained.Method,
            OpenApiEndpointInventory.NormalizeRoute(constrained.Route)));
    }

    [Fact]
    public void RestoredInventory_MatchesGeneratedDocumentAgain()
    {
        var original = OpenApiEndpointInventory.FromRuntime();
        var mutated = new HashSet<EndpointIdentity>(original);
        mutated.Add(Probe);

        AssertNotEqual(mutated, OpenApiEndpointInventory.FromGeneratedDocument());

        var restored = new HashSet<EndpointIdentity>(original);
        AssertEqual(restored, OpenApiEndpointInventory.FromGeneratedDocument());
    }

    private static void AssertEqual(HashSet<EndpointIdentity> runtime, HashSet<EndpointIdentity> document)
    {
        Assert.Equal(runtime.OrderBy(identity => identity.Method).ThenBy(identity => identity.Route),
            document.OrderBy(identity => identity.Method).ThenBy(identity => identity.Route));
    }

    private static void AssertNotEqual(HashSet<EndpointIdentity> runtime, HashSet<EndpointIdentity> document)
    {
        Assert.NotEqual(runtime.OrderBy(identity => identity.Method).ThenBy(identity => identity.Route),
            document.OrderBy(identity => identity.Method).ThenBy(identity => identity.Route));
    }
}
