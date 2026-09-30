using Novolis.Raylib;
using Novolis.Raylib.CodeGen;
using Novolis.Raylib.Colors;
using Novolis.Raylib.Interop;
using Novolis.Raylib.Rendering;

namespace Novolis.Raylib.CodeGen.Unit;

[NotInParallel("raylib-debug-capture-gate")]
public sealed class RaylibDebugCaptureGateTests
{
    [Test]
    public async Task IsRequested_true_when_programmatic_enabled()
    {
        RaylibDebugCaptureGate.ProgrammaticEnabled = true;
        try
        {
            await Assert.That(RaylibDebugCaptureGate.IsRequested("NOVOLIS_UNUSED")).IsTrue();
        }
        finally
        {
            RaylibDebugCaptureGate.ProgrammaticEnabled = false;
        }
    }

    [Test]
    public async Task IsRequested_honors_environment_variable()
    {
        RaylibDebugCaptureGate.ProgrammaticEnabled = false;
        Environment.SetEnvironmentVariable("NOVOLIS_CAPTURE_GATE_TEST", "yes");
        try
        {
            await Assert.That(RaylibDebugCaptureGate.IsRequested("NOVOLIS_CAPTURE_GATE_TEST")).IsTrue();
        }
        finally
        {
            Environment.SetEnvironmentVariable("NOVOLIS_CAPTURE_GATE_TEST", null);
        }
    }
}
