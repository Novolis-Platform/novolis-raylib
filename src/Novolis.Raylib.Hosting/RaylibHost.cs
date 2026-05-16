using Microsoft.Extensions.Hosting;

namespace Novolis.Raylib.Hosting;

/// <summary>Factory for a generic host configured with raylib loop services.</summary>
public static class RaylibHost
{
    public static HostApplicationBuilder CreateApplicationBuilder(string[]? args = null) =>
        Host.CreateApplicationBuilder(args ?? []);
}
