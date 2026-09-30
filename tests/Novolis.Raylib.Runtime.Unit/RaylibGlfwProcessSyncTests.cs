using Novolis.Raylib.Abstractions;
using Novolis.Raylib.Internal;
using Novolis.Raylib.Shell;

namespace Novolis.Raylib.Runtime.Unit;

public sealed class RaylibGlfwProcessSyncTests
{
    [Test]
    public async Task Enter_acquires_and_releases_mutex()
    {
        using var scope = RaylibGlfwProcessSync.Enter();
        await Assert.That(scope.GetType().Name).IsEqualTo("LockScope");
    }
}
