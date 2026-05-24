using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Novolis.Raylib.Hosting;

namespace Novolis.Raylib.Testing;

/// <summary>Builds an in-process <see cref="IHost"/> with raylib services for integration tests.</summary>
public static class RaylibHostingTestHost
{
    /// <summary>Builds a raylib host with optional configuration and service registration.</summary>
    /// <param name="configure">Optional raylib host options callback.</param>
    /// <param name="services">Optional extra DI registrations.</param>
    /// <returns>Built host ready to start.</returns>
    public static IHost Build(Action<RaylibHostOptions>? configure = null, Action<IServiceCollection>? services = null)
    {
        var builder = RaylibHost.CreateApplicationBuilder([]);
        builder.AddRaylib(configure);
        services?.Invoke(builder.Services);
        return builder.Build();
    }
}
