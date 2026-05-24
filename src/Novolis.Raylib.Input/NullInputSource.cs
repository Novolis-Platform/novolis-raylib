namespace Novolis.Raylib.Input;

/// <summary>No-op input for headless tests.</summary>
public sealed class NullInputSource : IInputSource
{
    public void OnMouseMove(Action<MouseEventArgs> callback) { }
    public void OnMouseClick(Action<MouseEventArgs> callback) { }
    public void OnKeyPress(Action<KeyboardEventArgs> callback) { }
    public void OnKeyRelease(Action<KeyboardEventArgs> callback) { }
    public void Start() { }
    public void Stop() { }
}
