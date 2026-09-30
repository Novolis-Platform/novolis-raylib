using Novolis.Raylib.Abstractions;
using Novolis.Raylib.Input;

namespace Novolis.Raylib.Testing.Unit;

public sealed class NullInputSourceTests
{
    [Test]
    public void StartStop_AreNoOps()
    {
        var input = new NullInputSource();
        input.Start();
        input.Stop();
    }

    [Test]
    public void EventSubscriptions_AcceptCallbacksWithoutInvoking()
    {
        var input = new NullInputSource();
        input.OnMouseMove(_ => throw new InvalidOperationException("Should not invoke"));
        input.OnMouseClick(_ => throw new InvalidOperationException("Should not invoke"));
        input.OnKeyPress(_ => throw new InvalidOperationException("Should not invoke"));
        input.OnKeyRelease(_ => throw new InvalidOperationException("Should not invoke"));
    }
}
