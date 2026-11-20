using System.Diagnostics;

namespace RealtorApiTests.OpenApi;

[Collection("OpenApi")]
public sealed class OpenApiDeterminismTests
{
    [Fact]
    public async Task Regeneration_ProducesByteIdenticalDocument()
    {
        var first = await RegenerateAndReadAsync();
        var second = await RegenerateAndReadAsync();

        Assert.Equal(first, second);
    }

    private static async Task<byte[]> RegenerateAndReadAsync()
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            WorkingDirectory = OpenApiTestApp.RepositoryRoot,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        startInfo.ArgumentList.Add("build");
        startInfo.ArgumentList.Add("app/backend/src/RealtorApi/RealtorApi.csproj");
        startInfo.ArgumentList.Add("--no-restore");

        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("No se pudo iniciar dotnet.");
        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        var output = (await outputTask) + Environment.NewLine + (await errorTask);
        Assert.True(process.ExitCode == 0, $"La regeneración falló: {output}");

        return await File.ReadAllBytesAsync(OpenApiTestApp.DocumentPath);
    }
}
