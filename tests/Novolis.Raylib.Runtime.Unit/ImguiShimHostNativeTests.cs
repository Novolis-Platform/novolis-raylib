using Novolis.Raylib.Testing;

namespace Novolis.Raylib.Runtime.Unit;

[Category("Native")]
[RunOnlyIfNativeRaylib]
[NotInParallel("raylib-glfw")]
public sealed class ImguiShimHostNativeTests
{
    [Test]
    public async Task EnsureInitialized_loads_imgui_shim()
    {
        using var scope = RaylibTestRuntime.EnterNativeOffscreen();
        ImguiShimHost.EnsureInitialized();
        ImguiShimHost.EnsureInitialized();
        await Task.CompletedTask;
    }
}
