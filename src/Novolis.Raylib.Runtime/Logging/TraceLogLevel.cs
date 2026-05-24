namespace Novolis.Raylib.Logging;

/// <summary>Trace log levels (same numeric values as the prior Raylib-CSharp wrapper).</summary>
public enum TraceLogLevel
{
    /// <summary>Log all messages.</summary>
    All = 0,

    /// <summary>Trace-level messages.</summary>
    Trace = 1,

    /// <summary>Debug-level messages.</summary>
    Debug = 2,

    /// <summary>Informational messages.</summary>
    Info = 3,

    /// <summary>Warning messages.</summary>
    Warning = 4,

    /// <summary>Error messages.</summary>
    Error = 5,

    /// <summary>Fatal messages.</summary>
    Fatal = 6,

    /// <summary>Disable trace logging.</summary>
    None = 7,
}
