namespace Novolis.Raylib.Abstractions;

/// <summary>Signals that the UI should redraw (event loop).</summary>
public interface IRaylibInvalidationSource
{
    bool IsInvalidated { get; }

    void ClearInvalidation();
}
