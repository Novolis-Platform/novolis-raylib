namespace Novolis.Raylib.Abstractions;

/// <summary>Per-frame hook for a Raylib host.</summary>
public interface IRaylibFrameRenderer
{
    /// <summary>Called each frame between <c>BeginDrawing</c> and <c>EndDrawing</c>.</summary>
    /// <param name="deltaSeconds">Frame delta time in seconds.</param>
    /// <param name="screenWidth">Current drawable width.</param>
    /// <param name="screenHeight">Current drawable height.</param>
    void OnFrame(float deltaSeconds, int screenWidth, int screenHeight);
}
