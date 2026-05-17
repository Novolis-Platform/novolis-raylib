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

    public bool Succeeded { get; }

    public bool RanNativeLoop { get; }

    public int FramesCompleted { get; }

    public byte[]? LastFramePng { get; }

    public IReadOnlyDictionary<int, byte[]> FramePngs { get; }

    public string? Message { get; }

    public static RaylibOffscreenTestRunResult Skipped(string reason) =>
        new(false, false, 0, null, new Dictionary<int, byte[]>(), reason);

    public static RaylibOffscreenTestRunResult Failed(string error) =>
        new(false, true, 0, null, new Dictionary<int, byte[]>(), error);

    public static RaylibOffscreenTestRunResult Ok(int frames, byte[]? png, IReadOnlyDictionary<int, byte[]>? framePngs = null) =>
        new(true, true, frames, png, framePngs ?? new Dictionary<int, byte[]>(), null);
}
