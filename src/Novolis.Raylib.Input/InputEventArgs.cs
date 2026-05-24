namespace Novolis.Raylib.Input;

/// <summary>Mouse event payload.</summary>
public readonly record struct MouseEventArgs(int X, int Y, int Button);

/// <summary>Keyboard event payload.</summary>
public readonly record struct KeyboardEventArgs(int KeyCode);
