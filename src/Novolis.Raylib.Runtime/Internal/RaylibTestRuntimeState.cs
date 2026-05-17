namespace Novolis.Raylib.Internal;

/// <summary>Scoped native offscreen test gating without environment variables.</summary>
internal static class RaylibTestRuntimeState
{
    private static int _assemblyNativeEnabled;
    private static readonly AsyncLocal<int> ScopeDepth = new();

    public static bool NativeOffscreenEnabled =>
        Volatile.Read(ref _assemblyNativeEnabled) != 0 || ScopeDepth.Value > 0;

    public static void EnableForAssembly() =>
        Volatile.Write(ref _assemblyNativeEnabled, 1);

    public static RaylibTestScope EnterNativeOffscreen()
    {
        ScopeDepth.Value++;
        return new RaylibTestScope();
    }

    internal sealed class RaylibTestScope : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            ScopeDepth.Value = Math.Max(0, ScopeDepth.Value - 1);
        }
    }
}
