using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Novolis.Raylib.Abstractions;
namespace Novolis.Raylib.Hosting;

internal sealed class RaylibHostedLoopService(
    RaylibHostOptions options,
    IRaylibShellRuntime shell,
    IServiceProvider services) : IHostedService
{
    private CancellationTokenSource? _cts;
    private Task? _loopTask;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _loopTask = Task.Run(() => RunLoop(_cts.Token), CancellationToken.None);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_cts is not null)
            await _cts.CancelAsync();
        if (_loopTask is not null)
            await _loopTask.WaitAsync(cancellationToken);
    }

    private int RunLoop(CancellationToken cancellationToken)
    {
        foreach (var startup in services.GetServices<IStartupSystem>())
            startup.OnStartup();

        try
        {
            return options.LoopModel switch
            {
                RaylibLoopModel.EventLoop => shell.RunShellFrame(options.WindowTitle, new EventLoopFrameRenderer(services), cancellationToken),
                _ => shell.RunShellFrame(options.WindowTitle, new RenderLoopFrameRenderer(services, options), cancellationToken),
            };
        }
        finally
        {
            foreach (var shutdown in services.GetServices<IShutdownSystem>())
                shutdown.OnShutdown();
        }
    }

    private sealed class RenderLoopFrameRenderer(IServiceProvider sp, RaylibHostOptions opt) : IRaylibFrameRenderer
    {
        private float _accumulator;

        public void OnFrame(float deltaSeconds, int screenWidth, int screenHeight)
        {
            foreach (var u in sp.GetServices<IUpdateSystem>())
                u.OnUpdate(deltaSeconds);

            _accumulator += deltaSeconds;
            while (_accumulator >= opt.FixedTimestepSeconds)
            {
                foreach (var f in sp.GetServices<IFixedUpdateSystem>())
                    f.OnFixedUpdate(opt.FixedTimestepSeconds);
                _accumulator -= opt.FixedTimestepSeconds;
            }

            foreach (var r in sp.GetServices<IRenderSystem>())
                r.OnRender(deltaSeconds, screenWidth, screenHeight);
        }
    }

    private sealed class EventLoopFrameRenderer(IServiceProvider sp) : IRaylibFrameRenderer
    {
        private bool _dirty = true;

        public void OnFrame(float deltaSeconds, int screenWidth, int screenHeight)
        {
            foreach (var inv in sp.GetServices<IRaylibInvalidationSource>())
            {
                if (inv.IsInvalidated)
                {
                    _dirty = true;
                    inv.ClearInvalidation();
                }
            }

            if (!_dirty)
                return;

            foreach (var r in sp.GetServices<IRenderSystem>())
                r.OnRender(deltaSeconds, screenWidth, screenHeight);
            _dirty = false;
        }
    }
}
