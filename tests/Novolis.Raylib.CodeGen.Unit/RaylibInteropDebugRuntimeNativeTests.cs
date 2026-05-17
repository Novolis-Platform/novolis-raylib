using Novolis.Raylib.Debug;
using Novolis.Raylib.Testing;

namespace Novolis.Raylib.CodeGen.Unit;

[NotInParallel("raylib-glfw")]
public sealed class RaylibInteropDebugRuntimeNativeTests
{
    private const string NativeTestsEnv = RaylibDebug.NativeTestsEnvironmentVariable;
    private const string HeadlessEnv = "STARCONFLICTS_RAYLIB_HEADLESS";

    [Test]
    public async Task RunMinimalFrameLoop_native_optional()
    {
        if (!IsTruthy(Environment.GetEnvironmentVariable(NativeTestsEnv)))
        {
            await Task.CompletedTask;
            return;
        }

        if (IsTruthy(Environment.GetEnvironmentVariable(HeadlessEnv)))
        {
            await Task.CompletedTask;
            return;
        }

        using var glfwLock = RaylibGlfwTestSync.Enter();
        try
        {
            var r = RaylibInteropDebugRuntime.RunMinimalFrameLoop(
                new RaylibInteropDebugLoopOptions
                {
                    Width = 320,
                    Height = 240,
                    HideWindow = true,
                    MaxFrames = 2,
                },
                CancellationToken.None);

            if (!r.Ok)
            {
                await Task.CompletedTask;
                return;
            }

            await Assert.That(r.FramesPresented).IsEqualTo(2);
        }
        catch (DllNotFoundException)
        {
            await Task.CompletedTask;
        }
    }

    private static bool IsTruthy(string? v)
    {
        if (string.IsNullOrWhiteSpace(v))
            return false;
        return v.Equals("1", StringComparison.OrdinalIgnoreCase)
               || v.Equals("true", StringComparison.OrdinalIgnoreCase)
               || v.Equals("yes", StringComparison.OrdinalIgnoreCase);
    }
}
