namespace Novolis.Raylib.Abstractions;

/// <summary>Runs a raylib window loop with a frame renderer.</summary>
public interface IRaylibShellRuntime
{
    /// <summary>Runs the window loop until the token is cancelled or the window closes.</summary>
    /// <param name="windowTitle">Initial window title.</param>
    /// <param name="frameRenderer">Receives per-frame callbacks inside <c>BeginDrawing</c>/<c>EndDrawing</c>.</param>
    /// <param name="cancellationToken">Stops the loop when cancelled.</param>
    /// <returns>Process exit code (0 on normal completion).</returns>
    int RunShellFrame(string windowTitle, IRaylibFrameRenderer frameRenderer, CancellationToken cancellationToken = default);
}
