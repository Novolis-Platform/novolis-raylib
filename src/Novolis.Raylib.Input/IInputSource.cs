namespace Novolis.Raylib.Input;

/// <summary>Platform input capture seam (keyboard/mouse). No Simulation dependency.</summary>
public interface IInputSource
{
    void OnMouseMove(Action<MouseEventArgs> callback);
    void OnMouseClick(Action<MouseEventArgs> callback);
    void OnKeyPress(Action<KeyboardEventArgs> callback);
    void OnKeyRelease(Action<KeyboardEventArgs> callback);
    void Start();
    void Stop();
}
