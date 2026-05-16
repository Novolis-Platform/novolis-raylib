using Microsoft.Extensions.DependencyInjection;
using Novolis.Raylib.Abstractions;
using Novolis.Raylib.Hosting;

namespace Novolis.Raylib.Hosting.Unit;

public class RaylibHostBuilderTests
{
    [Test]
    public async Task AddRaylib_registers_shell_and_loop_service()
    {
        var builder = RaylibHost.CreateApplicationBuilder([]);
        builder.AddRaylib(o => o.WindowTitle = "unit");
        using var host = builder.Build();
        await Assert.That(host.Services.GetService<IRaylibShellRuntime>()).IsNotNull();
        await Assert.That(host.Services.GetService<RaylibHostOptions>()?.WindowTitle).IsEqualTo("unit");
    }
}
