namespace Novolis.Raylib.Runtime.Presentation;

/// <summary>Opt-in callbacks after a frame is presented (<c>EndDrawing</c>). Zero cost when disabled.</summary>
public static class RaylibPresentationHooks
{
    private static volatile bool _enabled;
    private static Action? _onFramePresented;

    /// <summary>Registers a handler invoked on the render thread after each presented frame.</summary>
    public static void Register(Action? onFramePresented, bool enabled)
    {
        _onFramePresented = onFramePresented;
        _enabled = enabled && onFramePresented is not null;
    }

    /// <summary>Called from generated <c>Graphics.EndDrawing</c> after the native swap.</summary>
    public static void Notify()
    {
        if (!_enabled)
            return;

        _onFramePresented?.Invoke();
    }
}
