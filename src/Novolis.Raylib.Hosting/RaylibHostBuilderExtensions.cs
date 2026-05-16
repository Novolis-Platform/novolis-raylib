using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Novolis.Raylib.Abstractions;

namespace Novolis.Raylib.Hosting;

public static class RaylibHostBuilderExtensions
{
    public static IHostApplicationBuilder AddRaylib(this IHostApplicationBuilder builder, Action<RaylibHostOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        var options = new RaylibHostOptions();
        configure?.Invoke(options);
        builder.Services.AddSingleton(options);
        builder.Services.AddSingleton<IRaylibShellRuntime, RaylibShellRuntimeAdapter>();
        builder.Services.AddHostedService<RaylibHostedLoopService>();
        return builder;
    }

    public static IHostApplicationBuilder AddRaylibSystem<T>(this IHostApplicationBuilder builder)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(builder);
        var t = typeof(T);
        if (typeof(IStartupSystem).IsAssignableFrom(t))
            builder.Services.AddSingleton(typeof(IStartupSystem), t);
        if (typeof(IFixedUpdateSystem).IsAssignableFrom(t))
            builder.Services.AddSingleton(typeof(IFixedUpdateSystem), t);
        if (typeof(IUpdateSystem).IsAssignableFrom(t))
            builder.Services.AddSingleton(typeof(IUpdateSystem), t);
        if (typeof(IRenderSystem).IsAssignableFrom(t))
            builder.Services.AddSingleton(typeof(IRenderSystem), t);
        if (typeof(IShutdownSystem).IsAssignableFrom(t))
            builder.Services.AddSingleton(typeof(IShutdownSystem), t);
        builder.Services.AddSingleton(t);
        return builder;
    }
}
