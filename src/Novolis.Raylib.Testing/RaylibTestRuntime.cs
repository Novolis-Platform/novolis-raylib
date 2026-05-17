using Novolis.Raylib.Internal;

namespace Novolis.Raylib.Testing;

/// <summary>Public entry for native test runtime state (no environment variables).</summary>
public static class RaylibTestRuntime
{
    public static bool NativeOffscreenEnabled => RaylibTestRuntimeState.NativeOffscreenEnabled;

    public static void EnableForAssembly() => RaylibTestRuntimeState.EnableForAssembly();

    public static RaylibTestScope EnterNativeOffscreen() =>
        new(RaylibTestRuntimeState.EnterNativeOffscreen());

    public sealed class RaylibTestScope : IDisposable
    {
        private readonly RaylibTestRuntimeState.RaylibTestScope _inner;

        internal RaylibTestScope(RaylibTestRuntimeState.RaylibTestScope inner) => _inner = inner;

        public void Dispose() => _inner.Dispose();
    }
}
