using System.Reflection;
using System.Text;
using Novolis.Raylib.Testing;
using Novolis.Raylib.Testing.Golden;

namespace Novolis.Raylib.Testing.Unit;

public sealed class VisualCaptureArtifactsExtendedTests
{
    [Test]
    public async Task WritePng_throws_on_blank_file_name()
    {
        var threw = false;
        try
        {
#pragma warning disable CS0618
            VisualCaptureArtifacts.WritePng([], " ");
#pragma warning restore CS0618
        }
        catch (ArgumentException)
        {
            threw = true;
        }

        await Assert.That(threw).IsTrue();
    }

    [Test]
    public async Task RelativeCapturesDir_points_at_artifacts_folder()
    {
        await Assert.That(VisualCaptureArtifacts.RelativeCapturesDir)
            .IsEqualTo("artifacts/visual-captures");
    }
}
