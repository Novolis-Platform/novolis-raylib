namespace Novolis.Raylib.Abstractions;

/// <summary>Per-frame hook for a Raylib host.</summary>
public interface IRaylibFrameRenderer
{
    void OnFrame(float deltaSeconds, int screenWidth, int screenHeight);
}
