namespace Novolis.Raylib.Interop;

/// <summary>Framebuffer capture gate for generated <see cref="RaylibDebugFrameHooks"/> (no Runtime dependency).</summary>
internal static class RaylibDebugCaptureGate
{
    internal static volatile bool ProgrammaticEnabled;

    internal static bool IsRequested(string captureEnvironmentVariable)
    {
        if (ProgrammaticEnabled)
            return true;

        var v = Environment.GetEnvironmentVariable(captureEnvironmentVariable);
        if (string.IsNullOrWhiteSpace(v))
            return false;

        return v.Equals("1", StringComparison.OrdinalIgnoreCase)
               || v.Equals("true", StringComparison.OrdinalIgnoreCase)
               || v.Equals("yes", StringComparison.OrdinalIgnoreCase);
    }
}
