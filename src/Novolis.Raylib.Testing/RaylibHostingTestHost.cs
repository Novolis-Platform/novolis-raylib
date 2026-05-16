using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Novolis.Raylib.Hosting;

namespace Novolis.Raylib.Testing;

/// <summary>Builds an in-process <see cref="IHost"/> with raylib services for integration tests.</summary>
public static class RaylibHostingTestHost
{
    public static IHost Build(Action<RaylibHostOptions>? configure = null, Action<IServiceCollection>? services = null)
    {
        var builder = RaylibHost.CreateApplicationBuilder([]);
        builder.AddRaylib(configure);
        services?.Invoke(builder.Services);
        return builder.Build();
    }
}
