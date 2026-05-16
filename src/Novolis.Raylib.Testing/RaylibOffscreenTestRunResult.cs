namespace Novolis.Raylib.Testing;

/// <summary>Outcome of <see cref="RaylibOffscreenTestHarness.Run"/>.</summary>
public sealed class RaylibOffscreenTestRunResult
{
    private RaylibOffscreenTestRunResult(
        bool succeeded,
        bool ranNative,
        int framesCompleted,
        byte[]? lastFramePng,
        string? message)
    {
        Succeeded = succeeded;
        RanNativeLoop = ranNative;
        FramesCompleted = framesCompleted;
        LastFramePng = lastFramePng;
        Message = message;
    }

    public bool Succeeded { get; }

    public bool RanNativeLoop { get; }

    public int FramesCompleted { get; }

    public byte[]? LastFramePng { get; }

    public string? Message { get; }

    public static RaylibOffscreenTestRunResult Skipped(string reason) =>
        new(false, false, 0, null, reason);

    public static RaylibOffscreenTestRunResult Failed(string error) =>
        new(false, true, 0, null, error);

    public static RaylibOffscreenTestRunResult Ok(int frames, byte[]? png) =>
        new(true, true, frames, png, null);
}
