namespace Novolis.Raylib.Abstractions;

/// <summary>Invoked once when the raylib host starts.</summary>
public interface IStartupSystem
{
    /// <summary>Called once after the raylib host has started.</summary>
    void OnStartup();
}
