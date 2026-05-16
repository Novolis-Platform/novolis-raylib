using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;

namespace Novolis.Raylib.Interop;

/// <summary>UTF-8 marshaller for <c>[LibraryImport]</c> when runtime marshalling is disabled.</summary>
[CustomMarshaller(typeof(string), MarshalMode.ManagedToUnmanagedIn, typeof(Utf8StringMarshaller))]
public static unsafe class Utf8StringMarshaller
{
    public static nint ConvertToUnmanaged(string? managed)
    {
        if (managed is null)
            return 0;

        var byteCount = Encoding.UTF8.GetByteCount(managed);
        var ptr = Marshal.AllocHGlobal(byteCount + 1);
        var buffer = new Span<byte>((void*)ptr, byteCount + 1);
        var written = Encoding.UTF8.GetBytes(managed.AsSpan(), buffer);
        buffer[written] = 0;
        return ptr;
    }

    public static void Free(nint unmanaged)
    {
        if (unmanaged != 0)
            Marshal.FreeHGlobal(unmanaged);
    }
}
