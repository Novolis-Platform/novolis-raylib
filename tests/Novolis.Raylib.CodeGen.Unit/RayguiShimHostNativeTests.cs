using Novolis.Raylib.Testing;

namespace Novolis.Raylib.CodeGen.Unit;

[Category("Native")]
[RunOnlyIfNativeRaylib]
[NotInParallel("raylib-glfw")]
public sealed class RayguiShimHostNativeTests
{
    [Test]
    public async Task EnsureInitialized_loads_raygui_shim()
    {
        using var scope = RaylibTestRuntime.EnterNativeOffscreen();
        RayguiShimHost.EnsureInitialized();
        RayguiShimHost.EnsureInitialized();
        await Task.CompletedTask;
    }
}
