namespace Novolis.Raylib.Abstractions;

/// <summary>Fixed-timestep update phase (render loop).</summary>
public interface IFixedUpdateSystem
{
    void OnFixedUpdate(float fixedDeltaSeconds);
}
