namespace Novolis.Raylib.Abstractions;

/// <summary>Runs a raylib window loop with a frame renderer.</summary>
public interface IRaylibShellRuntime
{
    int RunShellFrame(string windowTitle, IRaylibFrameRenderer frameRenderer, CancellationToken cancellationToken = default);
}
