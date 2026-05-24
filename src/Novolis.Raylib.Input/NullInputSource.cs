namespace Novolis.Raylib.Input;

/// <summary>No-op input for headless tests.</summary>
public sealed class NullInputSource : IInputSource
{
    /// <inheritdoc />
    public void OnMouseMove(Action<MouseEventArgs> callback) { }

    /// <inheritdoc />
    public void OnMouseClick(Action<MouseEventArgs> callback) { }

    /// <inheritdoc />
    public void OnKeyPress(Action<KeyboardEventArgs> callback) { }

    /// <inheritdoc />
    public void OnKeyRelease(Action<KeyboardEventArgs> callback) { }

    /// <inheritdoc />
    public void Start() { }

    /// <inheritdoc />
    public void Stop() { }
}
