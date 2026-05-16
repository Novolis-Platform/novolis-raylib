using Microsoft.Extensions.Hosting;

namespace Novolis.Raylib.Hosting;

/// <summary>Factory for a generic host configured with raylib loop services.</summary>
public static class RaylibHost
{
    /// <summary>Creates a generic host builder for raylib apps (same as <see cref="Host.CreateApplicationBuilder"/>).</summary>
    /// <param name="args">Command-line arguments passed to configuration.</param>
    public static HostApplicationBuilder CreateApplicationBuilder(string[]? args = null) =>
        Host.CreateApplicationBuilder(args ?? []);
}
