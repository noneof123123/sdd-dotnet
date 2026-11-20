using System.Diagnostics;

namespace RealtorApiTests.OpenApi;

[Collection("OpenApi")]
public sealed class OpenApiScriptTests
{
    [Fact]
    public async Task Script_Success_ReportsFinalDocument()
    {
        var fixture = ScriptFixture.Create();
        try
        {
            var result = await RunAsync(fixture);

            Assert.Equal(0, result.ExitCode);
            Assert.Contains("[OK] build", result.Output);
            Assert.Contains("[OK] redocly lint", result.Output);
            Assert.Contains("[OK] nswag smoke client", result.Output);
            Assert.Contains("[OK] sin artefactos visuales", result.Output);
            Assert.Contains("app/backend/src/RealtorApi/wwwroot/openapi/v1.json", result.Output);
            Assert.True(File.Exists(fixture.ClientPath));
        }
        finally
        {
            fixture.Dispose();
        }
    }

    [Fact]
    public async Task Script_WhenDocumentIsMissing_Fails()
    {
        var fixture = ScriptFixture.Create();
        fixture.Environment["MISSING_DOCUMENT"] = "1";
        try
        {
            var result = await RunAsync(fixture);

            Assert.NotEqual(0, result.ExitCode);
            Assert.Contains("[ERROR]", result.Output);
            Assert.Contains("v1.json", result.Output);
        }
        finally
        {
            fixture.Dispose();
        }
    }

    [Fact]
    public async Task Script_WhenRedoclyConfigIsInvalid_Fails()
    {
        var fixture = ScriptFixture.Create();
        fixture.Environment["FAIL_NPX"] = "1";
        try
        {
            var result = await RunAsync(fixture);

            Assert.NotEqual(0, result.ExitCode);
            Assert.Contains("[ERROR] redocly lint", result.Output);
        }
        finally
        {
            fixture.Dispose();
        }
    }

    [Fact]
    public async Task Script_WhenNswagManifestIsInvalid_Fails()
    {
        var fixture = ScriptFixture.Create();
        fixture.Environment["FAIL_NSWAG"] = "1";
        try
        {
            var result = await RunAsync(fixture);

            Assert.NotEqual(0, result.ExitCode);
            Assert.Contains("[ERROR] nswag smoke client", result.Output);
        }
        finally
        {
            fixture.Dispose();
        }
    }

    [Fact]
    public async Task Script_WhenVisualArtifactAppears_Fails()
    {
        var fixture = ScriptFixture.Create();
        Directory.CreateDirectory(Path.GetDirectoryName(fixture.ClientPath)!);
        File.WriteAllText(Path.Combine(Path.GetDirectoryName(fixture.ClientPath)!, "index.html"), "ui");
        try
        {
            var result = await RunAsync(fixture);

            Assert.NotEqual(0, result.ExitCode);
            Assert.Contains("[ERROR] artifacts/openapi-client-smoke contiene archivos inesperados", result.Output);
        }
        finally
        {
            fixture.Dispose();
        }
    }

    private static async Task<ScriptResult> RunAsync(ScriptFixture fixture)
    {
        var startInfo = new ProcessStartInfo("/bin/sh")
        {
            WorkingDirectory = fixture.RepoRoot,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        startInfo.ArgumentList.Add(fixture.ScriptPath);
        startInfo.Environment["PATH"] = fixture.FakeBin + Path.PathSeparator + Environment.GetEnvironmentVariable("PATH");

        foreach (var pair in fixture.Environment)
        {
            startInfo.Environment[pair.Key] = pair.Value;
        }

        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("No se pudo iniciar el script.");
        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        return new ScriptResult(process.ExitCode, (await outputTask) + Environment.NewLine + (await errorTask));
    }

    private sealed record ScriptResult(int ExitCode, string Output);

    private sealed class ScriptFixture : IDisposable
    {
        private readonly string _root;

        private ScriptFixture(string root)
        {
            _root = root;
            RepoRoot = Path.Combine(root, "repo");
            FakeBin = Path.Combine(root, "bin");
            ScriptPath = Path.Combine(RepoRoot, "support", "scripts", "generate-openapi-v1.sh");
            ClientPath = Path.Combine(RepoRoot, "artifacts", "openapi-client-smoke", "RealtorApiClient.cs");
            Environment = new Dictionary<string, string>();
        }

        public string RepoRoot { get; }

        public string FakeBin { get; }

        public string ScriptPath { get; }

        public string ClientPath { get; }

        public Dictionary<string, string> Environment { get; }

        public static ScriptFixture Create()
        {
            var fixture = new ScriptFixture(Path.Combine(Path.GetTempPath(), "openapi-script-tests-" + Guid.NewGuid().ToString("N")));
            Directory.CreateDirectory(Path.Combine(fixture.RepoRoot, "support", "scripts"));
            Directory.CreateDirectory(Path.Combine(fixture.RepoRoot, ".config"));
            Directory.CreateDirectory(Path.Combine(fixture.RepoRoot, "app"));
            Directory.CreateDirectory(fixture.FakeBin);

            File.Copy(
                Path.Combine(OpenApiTestApp.RepositoryRoot, "support", "scripts", "generate-openapi-v1.sh"),
                fixture.ScriptPath);
            File.WriteAllText(Path.Combine(fixture.RepoRoot, ".redocly.yaml"), "extends: []\n");
            File.WriteAllText(Path.Combine(fixture.RepoRoot, ".config", "dotnet-tools.json"), "{}\n");
            File.WriteAllText(
                Path.Combine(fixture.RepoRoot, "support", "scripts", "openapi-v1.nswag.json"),
                "{}\n");
            File.WriteAllText(Path.Combine(fixture.RepoRoot, "app", "Realtor.sln"), string.Empty);

            WriteExecutable(Path.Combine(fixture.FakeBin, "node"), "#!/bin/sh\nexit 0\n");
            WriteExecutable(
                Path.Combine(fixture.FakeBin, "npx"),
                "#!/bin/sh\nif [ \"${FAIL_NPX:-0}\" = \"1\" ]; then exit 1; fi\nexit 0\n");
            WriteExecutable(
                Path.Combine(fixture.FakeBin, "dotnet"),
                """
                #!/bin/sh
                set -eu
                if [ "$1" = "build" ]; then
                    if [ "${MISSING_DOCUMENT:-0}" = "1" ]; then
                        exit 0
                    fi
                    mkdir -p app/backend/src/RealtorApi/wwwroot/openapi
                    printf '{"openapi":"3.0.4"}\n' > app/backend/src/RealtorApi/wwwroot/openapi/v1.json
                    exit 0
                fi
                if [ "$1" = "tool" ] && [ "$2" = "restore" ]; then
                    exit 0
                fi
                if [ "$1" = "tool" ] && [ "$2" = "run" ]; then
                    if [ "${FAIL_NSWAG:-0}" = "1" ]; then
                        exit 1
                    fi
                    mkdir -p artifacts/openapi-client-smoke
                    printf 'client\n' > artifacts/openapi-client-smoke/RealtorApiClient.cs
                    exit 0
                fi
                exit 1
                """);

            return fixture;
        }

        public void Dispose()
        {
            if (Directory.Exists(_root))
            {
                Directory.Delete(_root, recursive: true);
            }
        }

        private static void WriteExecutable(string path, string content)
        {
            File.WriteAllText(path, content);
            using var process = Process.Start(new ProcessStartInfo("chmod")
            {
                ArgumentList = { "+x", path },
                UseShellExecute = false,
                CreateNoWindow = true
            })!;
            process.WaitForExit();
        }
    }
}
