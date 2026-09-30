using Novolis.Raylib.Abstractions;
using Novolis.Raylib.Interact;
using Novolis.Raylib.Testing;
using Novolis.Raylib.Testing.Golden;

namespace Novolis.Raylib.Testing.Unit;

[NotInParallel("golden-adhoc-bucket")]
public sealed class GoldenRenderOutputLayoutTests
{
    [Test]
    public async Task ResetSharedRun_clears_bucket_state()
    {
        GoldenRenderOutputLayout.ResetSharedRun();
        await Assert.That(GoldenRenderOutputLayout.SharedRunFolder).IsNull();
    }
}
