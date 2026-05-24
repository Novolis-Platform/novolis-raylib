namespace Novolis.Raylib.Testing;

/// <summary>Outcome of <see cref="RaylibOffscreenTestHarness.Run"/>.</summary>
public sealed class RaylibOffscreenTestRunResult
{
    private RaylibOffscreenTestRunResult(
        bool succeeded,
        bool ranNative,
        int framesCompleted,
        byte[]? lastFramePng,
        IReadOnlyDictionary<int, byte[]> framePngs,
        string? message)
    {
        Succeeded = succeeded;
        RanNativeLoop = ranNative;
        FramesCompleted = framesCompleted;
        LastFramePng = lastFramePng;
        FramePngs = framePngs;
        Message = message;
    }

    /// <summary>True when the native loop completed without failure.</summary>
    public bool Succeeded { get; }

    /// <summary>True when the native raylib loop was entered (not skipped upfront).</summary>
    public bool RanNativeLoop { get; }

    /// <summary>Number of frames rendered before exit.</summary>
    public int FramesCompleted { get; }

    /// <summary>PNG bytes from the last captured frame, if any.</summary>
    public byte[]? LastFramePng { get; }

    /// <summary>1-based frame index to captured PNG bytes.</summary>
    public IReadOnlyDictionary<int, byte[]> FramePngs { get; }

    /// <summary>Skip or failure message when <see cref="Succeeded"/> is false.</summary>
    public string? Message { get; }

    /// <summary>Creates a skipped result (native loop not run).</summary>
    /// <param name="reason">Human-readable skip reason.</param>
    /// <returns>Skipped run result.</returns>
    public static RaylibOffscreenTestRunResult Skipped(string reason) =>
        new(false, false, 0, null, new Dictionary<int, byte[]>(), reason);

    /// <summary>Creates a failed result after entering the native loop.</summary>
    /// <param name="error">Failure message.</param>
    /// <returns>Failed run result.</returns>
    public static RaylibOffscreenTestRunResult Failed(string error) =>
        new(false, true, 0, null, new Dictionary<int, byte[]>(), error);

    /// <summary>Creates a successful run result with optional captures.</summary>
    /// <param name="frames">Frames completed.</param>
    /// <param name="png">Last captured PNG, if any.</param>
    /// <param name="framePngs">Per-frame captures keyed by 1-based frame index.</param>
    /// <returns>Successful run result.</returns>
    public static RaylibOffscreenTestRunResult Ok(int frames, byte[]? png, IReadOnlyDictionary<int, byte[]>? framePngs = null) =>
        new(true, true, frames, png, framePngs ?? new Dictionary<int, byte[]>(), null);
}
