namespace Novolis.Raylib.Abstractions;

/// <summary>Variable-timestep update phase (render loop).</summary>
public interface IUpdateSystem
{
    void OnUpdate(float deltaSeconds);
}
