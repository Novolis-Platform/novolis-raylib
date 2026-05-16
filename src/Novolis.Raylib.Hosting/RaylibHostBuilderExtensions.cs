using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Novolis.Raylib.Abstractions;

namespace Novolis.Raylib.Hosting;

/// <summary>Registers raylib window loop services on a generic host.</summary>
public static class RaylibHostBuilderExtensions
{
    /// <summary>Adds raylib shell, options, and a hosted loop service.</summary>
    /// <param name="builder">The application builder.</param>
    /// <param name="configure">Optional window and timing options.</param>
    /// <returns>The same builder for chaining.</returns>
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

    /// <summary>Registers a game system type against all matching phase interfaces it implements.</summary>
    /// <typeparam name="T">Concrete system type implementing one or more of <see cref="IStartupSystem"/>, <see cref="IUpdateSystem"/>, <see cref="IFixedUpdateSystem"/>, <see cref="IRenderSystem"/>, <see cref="IShutdownSystem"/>.</typeparam>
    /// <param name="builder">The application builder.</param>
    /// <returns>The same builder for chaining.</returns>
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
