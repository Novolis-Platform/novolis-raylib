using Microsoft.Extensions.Logging.Abstractions;
using Novolis.Raylib.Logging;
using Novolis.Raylib.Testing;

namespace Novolis.Raylib.Runtime.Unit;

public sealed class LoggerTests
{
    [Test]
    public async Task UseLoggerFactory_null_detaches_without_throw()
    {
        Logger.UseLoggerFactory(null);
        await Task.CompletedTask;
    }

    [Test]
    public async Task SetTraceLogLevel_accepts_enum_values()
    {
        Logger.SetTraceLogLevel(TraceLogLevel.Warning);
        await Task.CompletedTask;
    }
}

public sealed class VisualCaptureArtifactsTests
{
    [Test]
    public async Task FindRepoRoot_locates_slnx()
    {
        var root = VisualCaptureArtifacts.FindRepoRoot();
        await Assert.That(File.Exists(Path.Combine(root, "Novolis.Raylib.slnx"))).IsTrue();
    }
}
