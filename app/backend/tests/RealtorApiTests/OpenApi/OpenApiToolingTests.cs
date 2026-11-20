using System.Diagnostics;

namespace RealtorApiTests.OpenApi;

[Collection("OpenApi")]
public sealed class OpenApiToolingTests
{
    [Fact]
    public async Task RedoclyAndNSwag_AcceptGeneratedDocument()
    {
        Assert.True(File.Exists(OpenApiTestApp.DocumentPath));

        var redocly = await RunAsync(
            "npx",
            "--yes",
            "@redocly/cli",
            "lint",
            "--config",
            ".redocly.yaml",
            "app/backend/src/RealtorApi/wwwroot/openapi/v1.json");

        AssertToolSuccess("Redocly", redocly);

        var nswag = await RunAsync(
            "dotnet",
            "tool",
            "run",
            "nswag",
            "run",
            "support/scripts/openapi-v1.nswag.json");

        AssertToolSuccess("NSwag", nswag);

        var generatedClient = Path.Combine(
            OpenApiTestApp.RepositoryRoot,
            "artifacts",
            "openapi-client-smoke",
            "RealtorApiClient.cs");
        Assert.True(File.Exists(generatedClient));
    }

    private static void AssertToolSuccess(string toolName, ToolResult result)
    {
        Assert.True(
            result.ExitCode == 0,
            $"{toolName} falló con código {result.ExitCode}: {result.Output}");
    }

    private static async Task<ToolResult> RunAsync(string fileName, params string[] arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            WorkingDirectory = OpenApiTestApp.RepositoryRoot,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException($"No se pudo iniciar {fileName}.");
        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        var output = (await outputTask) + Environment.NewLine + (await errorTask);
        return new ToolResult(process.ExitCode, output);
    }

    private sealed record ToolResult(int ExitCode, string Output);
}
