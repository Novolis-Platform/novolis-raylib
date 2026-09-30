using System.Runtime.InteropServices;
using Novolis.CodeGen.Bindings;
using Novolis.Raylib.CodeGen;
using Novolis.Raylib.Interop;

namespace Novolis.Raylib.CodeGen.Unit;

public sealed class Utf8StringMarshallerTests
{
    [Test]
    public async Task ConvertToUnmanaged_round_trips_utf8()
    {
        var ptr = Utf8StringMarshaller.ConvertToUnmanaged("hello");
        try
        {
            await Assert.That(ptr).IsNotEqualTo(nint.Zero);
            var roundTrip = Marshal.PtrToStringUTF8(ptr);
            await Assert.That(roundTrip).IsEqualTo("hello");
        }
        finally
        {
            Utf8StringMarshaller.Free(ptr);
        }
    }

    [Test]
    public async Task ConvertToUnmanaged_null_returns_zero()
    {
        var ptr = Utf8StringMarshaller.ConvertToUnmanaged(null);
        await Assert.That(ptr).IsEqualTo(nint.Zero);
    }

    [Test]
    public async Task Free_zero_is_noop()
    {
        Utf8StringMarshaller.Free(0);
        await Task.CompletedTask;
    }
}
