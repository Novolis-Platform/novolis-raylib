namespace Novolis.Raylib.Abstractions;

/// <summary>Variable-timestep update phase (render loop).</summary>
public interface IUpdateSystem
{
    /// <summary>Variable-timestep logic update.</summary>
    /// <param name="deltaSeconds">Elapsed time since the last update in seconds.</param>
    void OnUpdate(float deltaSeconds);
}
