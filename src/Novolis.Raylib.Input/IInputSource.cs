namespace Novolis.Raylib.Input;

/// <summary>Platform input capture seam (keyboard/mouse). No Simulation dependency.</summary>
public interface IInputSource
{
    /// <summary>Registers a mouse-move handler.</summary>
    void OnMouseMove(Action<MouseEventArgs> callback);

    /// <summary>Registers a mouse-click handler.</summary>
    void OnMouseClick(Action<MouseEventArgs> callback);

    /// <summary>Registers a key-press handler.</summary>
    void OnKeyPress(Action<KeyboardEventArgs> callback);

    /// <summary>Registers a key-release handler.</summary>
    void OnKeyRelease(Action<KeyboardEventArgs> callback);

    /// <summary>Starts listening.</summary>
    void Start();

    /// <summary>Stops listening.</summary>
    void Stop();
}
