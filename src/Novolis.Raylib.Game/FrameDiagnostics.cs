using System.Diagnostics;

namespace Novolis.Raylib.Game;

/// <summary>Per-frame process metrics for debug overlays.</summary>
public sealed class FrameDiagnostics
{
    public float SmoothedFps { get; init; }

    public float FrameMilliseconds { get; init; }

    public double WorkingSetMegabytes { get; init; }

    public double GcHeapMegabytes { get; init; }

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
