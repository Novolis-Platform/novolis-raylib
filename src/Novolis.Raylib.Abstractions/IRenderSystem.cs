namespace Novolis.Raylib.Abstractions;

/// <summary>Draw phase (render loop).</summary>
public interface IRenderSystem
{
    void OnRender(float deltaSeconds, int screenWidth, int screenHeight);
}
