using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Novolis.Raylib.Abstractions;
using Novolis.Raylib.Hosting;

namespace Novolis.Raylib.Hosting.Unit;

public sealed class RaylibHostedLoopServiceTests
{
    [Test]
    public async Task Hosted_loop_invokes_startup_render_and_shutdown()
    {
        var trace = new PhaseTrace();
        var builder = RaylibHost.CreateApplicationBuilder([]);
        builder.AddRaylib(o =>
        {
            o.WindowTitle = "hosted-unit";
            o.LoopModel = RaylibLoopModel.RenderLoop;
            o.FixedTimestepSeconds = 1f / 60f;
        });
        builder.Services.AddSingleton<IStartupSystem>(trace);
        builder.Services.AddSingleton<IUpdateSystem>(trace);
        builder.Services.AddSingleton<IRenderSystem>(trace);
        builder.Services.AddSingleton<IShutdownSystem>(trace);
        builder.Services.AddSingleton<IRaylibShellRuntime, FakeShellRuntime>();

        using var host = builder.Build();
        await host.StartAsync();
        await Task.Delay(200);
        await host.StopAsync();

        await Assert.That(trace.StartupCount).IsEqualTo(1);
        await Assert.That(trace.RenderCount).IsGreaterThanOrEqualTo(1);
        await Assert.That(trace.UpdateCount).IsGreaterThanOrEqualTo(1);
        await Assert.That(trace.ShutdownCount).IsEqualTo(1);
    }

    [Test]
    public async Task Event_loop_skips_render_until_invalidated()
    {
        var trace = new PhaseTrace();
        var invalidation = new FakeInvalidationSource();
        var builder = RaylibHost.CreateApplicationBuilder([]);
        builder.AddRaylib(o => o.LoopModel = RaylibLoopModel.EventLoop);
        builder.Services.AddSingleton<IRenderSystem>(trace);
        builder.Services.AddSingleton<IRaylibInvalidationSource>(invalidation);
        builder.Services.AddSingleton<IRaylibShellRuntime, MultiFrameShellRuntime>();

        using var host = builder.Build();
        await host.StartAsync();
        await Task.Delay(200);
        await host.StopAsync();

        await Assert.That(trace.RenderCount).IsGreaterThanOrEqualTo(1);
    }

    private sealed class PhaseTrace : IStartupSystem, IUpdateSystem, IRenderSystem, IShutdownSystem
    {
        public int StartupCount { get; private set; }
        public int UpdateCount { get; private set; }
        public int RenderCount { get; private set; }
        public int ShutdownCount { get; private set; }

        public void OnStartup() => StartupCount++;
        public void OnUpdate(float deltaSeconds) => UpdateCount++;
        public void OnRender(float deltaSeconds, int screenWidth, int screenHeight) => RenderCount++;
        public void OnShutdown() => ShutdownCount++;
    }

    private sealed class FakeInvalidationSource : IRaylibInvalidationSource
    {
        private bool _dirty = true;
        public bool IsInvalidated => _dirty;
        public void ClearInvalidation() => _dirty = false;
        public void Invalidate() => _dirty = true;
    }

    private sealed class FakeShellRuntime : IRaylibShellRuntime
    {
        public int RunShellFrame(string windowTitle, IRaylibFrameRenderer frameRenderer, CancellationToken cancellationToken = default)
        {
            frameRenderer.OnFrame(1f / 60f, 800, 600);
            return 0;
        }
    }

    private sealed class MultiFrameShellRuntime : IRaylibShellRuntime
    {
        public int RunShellFrame(string windowTitle, IRaylibFrameRenderer frameRenderer, CancellationToken cancellationToken = default)
        {
            for (var i = 0; i < 3 && !cancellationToken.IsCancellationRequested; i++)
                frameRenderer.OnFrame(1f / 60f, 800, 600);
            return 0;
        }
    }
}
