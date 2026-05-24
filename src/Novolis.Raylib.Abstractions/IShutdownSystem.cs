namespace Novolis.Raylib.Abstractions;

/// <summary>Invoked when the raylib host shuts down.</summary>
public interface IShutdownSystem
{
    /// <summary>Called once when the raylib host is shutting down.</summary>
    void OnShutdown();
}
