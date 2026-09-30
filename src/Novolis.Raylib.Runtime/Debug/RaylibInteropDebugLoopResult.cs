namespace Novolis.Raylib.Debug;

/// <inheritdoc cref="RaylibDebug.LoopResult"/>
/// <param name="Ok">Whether the window initialized successfully.</param>
/// <param name="Width">Window width used.</param>
/// <param name="Height">Window height used.</param>
/// <param name="FramesPresented">Number of frames presented.</param>
public readonly record struct RaylibInteropDebugLoopResult(bool Ok, int Width, int Height, int FramesPresented);
