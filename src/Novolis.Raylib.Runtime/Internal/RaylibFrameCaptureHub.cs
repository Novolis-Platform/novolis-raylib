namespace Novolis.Raylib.Internal;

/// <summary>Opt-in per-frame notify after EndDrawing; zero cost when disabled.</summary>
internal static class RaylibFrameCaptureHub
{
    private static volatile bool _enabled;
    private static Action? _onFramePresented;

    public static void Register(Action? handler, bool enabled)
    {
        _onFramePresented = handler;
        _enabled = enabled && handler is not null;
    }

    public static void Notify()
    {
        if (!_enabled)
            return;

        _onFramePresented?.Invoke();
    }
}
