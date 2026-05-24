using System.Diagnostics;

namespace Novolis.Raylib.Game;

/// <summary>Per-frame process metrics for debug overlays.</summary>
public sealed class FrameDiagnostics
{
    /// <summary>Smoothed frames per second.</summary>
    public float SmoothedFps { get; init; }

    /// <summary>Last frame duration in milliseconds.</summary>
    public float FrameMilliseconds { get; init; }

    /// <summary>Process working set in megabytes.</summary>
    public double WorkingSetMegabytes { get; init; }

    /// <summary>Managed GC heap size in megabytes.</summary>
    public double GcHeapMegabytes { get; init; }

    /// <summary>Captures current process metrics for overlay display.</summary>
    /// <param name="smoothedFps">Smoothed FPS value.</param>
    /// <param name="deltaSeconds">Frame delta time in seconds.</param>
    /// <returns>Populated diagnostics snapshot.</returns>
    public static FrameDiagnostics Capture(float smoothedFps, float deltaSeconds)
    {
        var proc = Process.GetCurrentProcess();
        return new FrameDiagnostics
        {
            SmoothedFps = smoothedFps,
            FrameMilliseconds = deltaSeconds * 1000f,
            WorkingSetMegabytes = proc.WorkingSet64 / (1024.0 * 1024.0),
            GcHeapMegabytes = GC.GetTotalMemory(false) / (1024.0 * 1024.0),
        };
    }
}
