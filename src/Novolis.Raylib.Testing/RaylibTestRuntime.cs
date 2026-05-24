using Novolis.Raylib.Internal;

namespace Novolis.Raylib.Testing;

/// <summary>Public entry for native test runtime state (no environment variables).</summary>
public static class RaylibTestRuntime
{
    /// <summary>True when native offscreen is enabled for the current assembly or scope.</summary>
    public static bool NativeOffscreenEnabled => RaylibTestRuntimeState.NativeOffscreenEnabled;

    /// <summary>Enables native offscreen for all tests in the calling assembly.</summary>
    public static void EnableForAssembly() => RaylibTestRuntimeState.EnableForAssembly();

    /// <summary>Enables native offscreen until the returned scope is disposed.</summary>
    /// <returns>Disposable scope that restores prior state.</returns>
    public static RaylibTestScope EnterNativeOffscreen() =>
        new(RaylibTestRuntimeState.EnterNativeOffscreen());

    /// <summary>Restores native offscreen state when disposed.</summary>
    public sealed class RaylibTestScope : IDisposable
    {
        private readonly RaylibTestRuntimeState.RaylibTestScope _inner;

        internal RaylibTestScope(RaylibTestRuntimeState.RaylibTestScope inner) => _inner = inner;

        /// <inheritdoc />
        public void Dispose() => _inner.Dispose();
    }
}
