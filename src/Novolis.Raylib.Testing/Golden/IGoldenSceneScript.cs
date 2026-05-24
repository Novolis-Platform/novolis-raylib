namespace Novolis.Raylib.Testing.Golden;

/// <summary>Configures application state before each golden frame harness sub-run.</summary>
public interface IGoldenSceneScript
{
    /// <summary>Prepares scene state before a frame sub-run.</summary>
    /// <param name="frameId">Frame identifier from the golden spec.</param>
    void BeginFrame(string frameId);
}
