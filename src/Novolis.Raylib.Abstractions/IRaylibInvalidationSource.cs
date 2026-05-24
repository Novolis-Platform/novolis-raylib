namespace Novolis.Raylib.Abstractions;

/// <summary>Signals that the UI should redraw (event loop).</summary>
public interface IRaylibInvalidationSource
{
    /// <summary>Gets whether a redraw has been requested.</summary>
    bool IsInvalidated { get; }

    /// <summary>Clears the invalidation flag after the UI has processed a redraw.</summary>
    void ClearInvalidation();
}
