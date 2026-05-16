namespace Novolis.Raylib.Abstractions;

/// <summary>Invoked when the raylib host shuts down.</summary>
public interface IShutdownSystem
{
    void OnShutdown();
}
