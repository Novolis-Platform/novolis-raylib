using Novolis.Raylib.Abstractions;
using Novolis.Raylib.Interact;
using Novolis.Raylib.Testing;
using Novolis.Raylib.Testing.Golden;

namespace Novolis.Raylib.Testing.Unit;

[NotInParallel("novolis-test-gate-env")]
public sealed class GoldenTestGateTests
{
    [Test]
    public async Task IsOptInEnabled_true_for_one_and_true()
    {
        Environment.SetEnvironmentVariable("NOVOLIS_TEST_GATE", "1");
        try
        {
            await Assert.That(GoldenTestGate.IsOptInEnabled("NOVOLIS_TEST_GATE")).IsTrue();
        }
        finally
        {
            Environment.SetEnvironmentVariable("NOVOLIS_TEST_GATE", null);
        }

        Environment.SetEnvironmentVariable("NOVOLIS_TEST_GATE", "true");
        try
        {
            await Assert.That(GoldenTestGate.IsOptInEnabled("NOVOLIS_TEST_GATE")).IsTrue();
        }
        finally
        {
            Environment.SetEnvironmentVariable("NOVOLIS_TEST_GATE", null);
        }
    }

    [Test]
    public async Task IsOptInEnabled_false_when_unset()
    {
        Environment.SetEnvironmentVariable("NOVOLIS_TEST_GATE", null);
        try
        {
            await Assert.That(GoldenTestGate.IsOptInEnabled("NOVOLIS_TEST_GATE")).IsFalse();
        }
        finally
        {
            Environment.SetEnvironmentVariable("NOVOLIS_TEST_GATE", null);
        }
    }
}
