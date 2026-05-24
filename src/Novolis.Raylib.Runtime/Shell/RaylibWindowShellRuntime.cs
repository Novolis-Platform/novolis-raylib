using Novolis.Raylib.Abstractions;

namespace Novolis.Raylib.Shell;

/// <summary>Default <see cref="IRaylibShellRuntime"/> backed by <see cref="RaylibRuntimeShell"/>.</summary>
public sealed class RaylibWindowShellRuntime : IRaylibShellRuntime
{
    /// <inheritdoc />
    public int RunShellFrame(string windowTitle, IRaylibFrameRenderer frameRenderer, CancellationToken cancellationToken = default) =>
        RaylibRuntimeShell.RunShellFrame(windowTitle, frameRenderer, cancellationToken);
}
