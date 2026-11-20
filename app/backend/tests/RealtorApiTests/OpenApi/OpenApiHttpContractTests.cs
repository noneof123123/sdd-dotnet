using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace RealtorApiTests.OpenApi;

[Collection("OpenApi")]
public sealed class OpenApiHttpContractTests
{
    [Fact]
    public async Task GetOpenApiDocument_ServesStaticJsonFile()
    {
        Assert.True(File.Exists(OpenApiTestApp.DocumentPath));

        await using var app = OpenApiTestApp.CreateStaticFileApp();
        var requestDelegate = ((IApplicationBuilder)app).Build();
        var context = new DefaultHttpContext
        {
            RequestServices = app.Services,
            Response = { Body = new MemoryStream() }
        };

        context.Request.Method = "GET";
        context.Request.Path = "/openapi/v1.json";
        await requestDelegate(context);

        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        Assert.Equal("application/json", context.Response.ContentType);

        context.Response.Body.Position = 0;
        using var document = await JsonDocument.ParseAsync(context.Response.Body);
        Assert.Equal("v1", document.RootElement.GetProperty("info").GetProperty("version").GetString());
    }
}
