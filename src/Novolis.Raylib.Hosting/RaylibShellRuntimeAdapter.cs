using Novolis.Raylib.Abstractions;
using Novolis.Raylib.Shell;

namespace Novolis.Raylib.Hosting;

internal sealed class RaylibShellRuntimeAdapter : IRaylibShellRuntime
{
    public int RunShellFrame(string windowTitle, IRaylibFrameRenderer frameRenderer, CancellationToken cancellationToken = default) =>
        RaylibRuntimeShell.RunShellFrame(windowTitle, frameRenderer, cancellationToken);
}
