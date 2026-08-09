using Microsoft.Extensions.DependencyInjection;
using Novolis.Raylib.Abstractions;
using Novolis.Raylib.Hosting;
using Novolis.Raylib.Shell;

namespace Novolis.Raylib.Hosting.Unit;

public sealed class HostingExtensionsTests
{
    [Test]
    public async Task AddRaylibSystem_registers_phase_interfaces()
    {
        var builder = RaylibHost.CreateApplicationBuilder([]);
        builder.AddRaylibSystem<CompositeSystem>();
        using var host = builder.Build();

        await Assert.That(host.Services.GetService<IStartupSystem>()).IsNotNull();
        await Assert.That(host.Services.GetService<IUpdateSystem>()).IsNotNull();
        await Assert.That(host.Services.GetService<IRenderSystem>()).IsNotNull();
        await Assert.That(host.Services.GetService<IShutdownSystem>()).IsNotNull();
        await Assert.That(host.Services.GetService<CompositeSystem>()).IsNotNull();
    }

    [Test]
    [NotInParallel("raylib-headless-env")]
    public async Task Shell_runtime_adapter_runs_headless_without_window()
    {
        Environment.SetEnvironmentVariable(
            RaylibRuntimeShell.HeadlessEnvironmentVariable,
            "1",
            EnvironmentVariableTarget.Process);
        try
        {
            var builder = RaylibHost.CreateApplicationBuilder([]);
            builder.AddRaylib();
            using var host = builder.Build();
            var shell = host.Services.GetRequiredService<IRaylibShellRuntime>();
            var code = shell.RunShellFrame("adapter-test", new NoOpRenderer());
            await Assert.That(code).IsEqualTo(0);
        }
        finally
        {
            Environment.SetEnvironmentVariable(
                RaylibRuntimeShell.HeadlessEnvironmentVariable,
                null,
                EnvironmentVariableTarget.Process);
        }
    }

    [Test]
    public async Task AddRaylib_throws_when_builder_null()
    {
        var threw = false;
        try
        {
            RaylibHostBuilderExtensions.AddRaylib(null!);
        }
        catch (ArgumentNullException)
        {
            threw = true;
        }

        await Assert.That(threw).IsTrue();
    }

    private sealed class CompositeSystem : IStartupSystem, IUpdateSystem, IRenderSystem, IShutdownSystem
    {
        public void OnStartup() { }
        public void OnUpdate(float deltaSeconds) { }
        public void OnRender(float deltaSeconds, int screenWidth, int screenHeight) { }
        public void OnShutdown() { }
    }

    private sealed class NoOpRenderer : IRaylibFrameRenderer
    {
        public void OnFrame(float deltaSeconds, int screenWidth, int screenHeight) { }
    }
}
