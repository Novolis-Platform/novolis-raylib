using Novolis.Raylib.Debug;
using Novolis.Raylib.Interop;

namespace Novolis.Raylib.CodeGen.Unit;

/// <summary>Optional smoke tests that load <c>raylib.dll</c>; default CI does not set the env gate.</summary>
public sealed class RaylibNativeSmokeTests
{
    private const string NativeTestsEnv = RaylibDebug.NativeTestsEnvironmentVariable;
    private const string HeadlessEnv = "STARCONFLICTS_RAYLIB_HEADLESS";

    [Test]
    public async Task Init_minimal_window_when_native_tests_enabled()
    {
        if (!IsTruthy(NativeTestsEnv) || IsTruthy(HeadlessEnv))
        {
            await Task.CompletedTask;
            return;
        }

        try
        {
            Raylib6Native.InitWindow(64, 64, "Novolis.Raylib.CodeGen.Unit");
            var w = Raylib6Native.GetScreenWidth();
            await Assert.That(w).IsGreaterThanOrEqualTo(64);
            Raylib6Native.CloseWindow();
        }
        catch (DllNotFoundException)
        {
            await Task.CompletedTask;
        }
    }

    private static bool IsTruthy(string name)
    {
        var v = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrWhiteSpace(v))
            return false;
        return v.Equals("1", StringComparison.OrdinalIgnoreCase)
               || v.Equals("true", StringComparison.OrdinalIgnoreCase)
               || v.Equals("yes", StringComparison.OrdinalIgnoreCase);
    }
}
