using Microsoft.Extensions.Logging.Abstractions;
using Novolis.Raylib.Logging;
using Novolis.Raylib.Testing;

namespace Novolis.Raylib.Runtime.Unit;

public sealed class VisualCaptureArtifactsTests
{
    [Test]
    public async Task FindRepoRoot_locates_slnx()
    {
        var root = VisualCaptureArtifacts.FindRepoRoot();
        await Assert.That(File.Exists(Path.Combine(root, "Novolis.Raylib.slnx"))).IsTrue();
    }
}
