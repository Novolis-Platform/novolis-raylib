using Novolis.Raylib.CodeGen;
using Novolis.Raylib.Pipeline;
using Novolis.Raylib.Pipeline.Steps;

namespace Novolis.Raylib.Pipeline.Unit;

public sealed class ProcessRunnerTests
{
    [Test]
    public async Task RunAsync_returns_process_exit_code_and_logs_command()
    {
        var tempRoot = CreateTempRepoRoot();
        try
        {
            var log = new StringWriter();
            var layout = new RaylibPipelineLayout(tempRoot);
            var context = new PipelineContext { Layout = layout, Log = log, Force = false };

            var (fileName, arguments) = OperatingSystem.IsWindows()
                ? ("cmd.exe", "/c exit 42")
                : ("/bin/sh", "-c \"exit 42\"");

            var code = await ProcessRunner.RunAsync(context, fileName, arguments, tempRoot, CancellationToken.None);
            await Assert.That(code).IsEqualTo(42);
            await Assert.That(log.ToString()).Contains(fileName);
        }
        finally
        {
            Directory.Delete(tempRoot, recursive: true);
        }
    }

    [Test]
    public async Task RunAsync_captures_stdout()
    {
        var tempRoot = CreateTempRepoRoot();
        try
        {
            var log = new StringWriter();
            var layout = new RaylibPipelineLayout(tempRoot);
            var context = new PipelineContext { Layout = layout, Log = log, Force = false };

            var (fileName, arguments) = OperatingSystem.IsWindows()
                ? ("cmd.exe", "/c echo pipeline-stdout")
                : ("/bin/sh", "-c \"echo pipeline-stdout\"");

            var code = await ProcessRunner.RunAsync(context, fileName, arguments, tempRoot, CancellationToken.None);
            await Assert.That(code).IsEqualTo(0);
            await Assert.That(log.ToString()).Contains("pipeline-stdout");
        }
        finally
        {
            Directory.Delete(tempRoot, recursive: true);
        }
    }

    private static string CreateTempRepoRoot()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "novolis-pipeline-unit", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot);
        File.WriteAllText(Path.Combine(tempRoot, "Directory.Packages.props"), "<Project/>");
        return tempRoot;
    }
}
