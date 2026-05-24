using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Extensions.Logging;
using Novolis.Raylib;
using Novolis.Raylib.Interop;

namespace Novolis.Raylib.Logging;

/// <summary>raylib trace level + optional bridge to <see cref="ILogger"/> via the native shim in <c>novolis_raylib_trace</c>.</summary>
public static class Logger
{
    private const string DefaultCategoryName = "Novolis.Raylib.Interop";

    private static readonly object Gate = new();

    private static ILogger? s_logger;

    /// <summary>Registers or clears forwarding of raylib <c>TraceLog</c> into Microsoft.Extensions.Logging. Pass <c>null</c> to detach.</summary>
    /// <remarks>Requires <see cref="RaylibTraceHost.EnsureInitialized"/> (loads <c>novolis_raylib_trace</c>) when <paramref name="factory"/> is non-null.</remarks>
    public static unsafe void UseLoggerFactory(ILoggerFactory? factory, string? categoryName = null)
    {
        lock (Gate)
        {
            try
            {
                NovolisRaylibTraceForwarderNative.Clear();
            }
            catch (DllNotFoundException)
            {
            }

            Volatile.Write(ref s_logger, null);

            if (factory is null)
                return;

            RaylibTraceHost.EnsureInitialized();

            Volatile.Write(
                ref s_logger,
                factory.CreateLogger(string.IsNullOrEmpty(categoryName) ? DefaultCategoryName : categoryName!));
            NovolisRaylibTraceForwarderNative.Install(&OnNativeTrace);
        }
    }

    /// <summary>Sets the native raylib minimum trace log level.</summary>
    /// <param name="level">Minimum level forwarded to raylib.</param>
    public static void SetTraceLogLevel(TraceLogLevel level) => Raylib6Native.SetTraceLogLevel((int)level);

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe void OnNativeTrace(int raylibLogLevel, byte* messageUtf8)
    {
        var logger = Volatile.Read(ref s_logger);
        if (logger is null)
            return;

        var msg = Marshal.PtrToStringUTF8((nint)messageUtf8) ?? string.Empty;
        var level = MapRaylibToLogLevel(raylibLogLevel);
        if (level == LogLevel.None)
            return;

        logger.Log(level, "{RaylibTraceMessage}", msg);
    }

    private static LogLevel MapRaylibToLogLevel(int raylibLogLevel) =>
        raylibLogLevel switch
        {
            (int)TraceLogLevel.Fatal => LogLevel.Critical,
            (int)TraceLogLevel.Error => LogLevel.Error,
            (int)TraceLogLevel.Warning => LogLevel.Warning,
            (int)TraceLogLevel.Info => LogLevel.Information,
            (int)TraceLogLevel.Debug => LogLevel.Debug,
            (int)TraceLogLevel.Trace => LogLevel.Trace,
            (int)TraceLogLevel.All => LogLevel.Trace,
            (int)TraceLogLevel.None => LogLevel.None,
            _ => LogLevel.Information,
        };
}
