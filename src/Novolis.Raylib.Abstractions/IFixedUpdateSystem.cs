namespace Novolis.Raylib.Abstractions;

/// <summary>Fixed-timestep update phase (render loop).</summary>
public interface IFixedUpdateSystem
{
    /// <summary>Fixed-timestep logic update.</summary>
    /// <param name="fixedDeltaSeconds">Configured fixed timestep in seconds.</param>
    void OnFixedUpdate(float fixedDeltaSeconds);
}
