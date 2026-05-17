namespace Novolis.Raylib.Testing.Golden;

/// <summary>Configures application state before each golden frame harness sub-run.</summary>
public interface IGoldenSceneScript
{
    void BeginFrame(string frameId);
}
