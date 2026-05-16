using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Novolis.Raylib.Interop;

/// <summary>Native shim in <c>novolis_raygui</c> that formats <c>TraceLogCallback</c> (<c>va_list</c>) and forwards to an unmanaged delegate.</summary>
internal static unsafe partial class NovolisRaylibTraceForwarderNative
{
    private const string ShimDll = "novolis_raygui";

    [LibraryImport(ShimDll, EntryPoint = "NOVOLIS_RAYLIB_trace_forwarder_install")]
    internal static partial void Install(delegate* unmanaged[Cdecl]<int, byte*, void> callback);

    [LibraryImport(ShimDll, EntryPoint = "NOVOLIS_RAYLIB_trace_forwarder_clear")]
    internal static partial void Clear();
}
