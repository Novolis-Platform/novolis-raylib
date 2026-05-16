namespace Novolis.Raylib.Abstractions;

/// <summary>Draw phase (render loop).</summary>
public interface IRenderSystem
{
    /// <summary>Draw phase; use Runtime façades such as Graphics, World, Hud, and Gui.</summary>
    /// <param name="deltaSeconds">Frame delta time in seconds.</param>
    /// <param name="screenWidth">Current drawable width.</param>
    /// <param name="screenHeight">Current drawable height.</param>
    void OnRender(float deltaSeconds, int screenWidth, int screenHeight);
}
