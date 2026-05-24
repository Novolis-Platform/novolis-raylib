using Novolis.Raylib.Abstractions;
using Novolis.Raylib.Interact;
using Novolis.Raylib.Testing;
using Novolis.Raylib.Testing.Golden;

namespace Novolis.Raylib.Testing.Unit;

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
        await Assert.That(GoldenTestGate.IsOptInEnabled("NOVOLIS_TEST_GATE")).IsFalse();
    }
}

public sealed class DeterministicFrameClockTests
{
    [Test]
    public async Task Step_advances_time_by_delta()
    {
        var clock = new DeterministicFrameClock();
        clock.SetDelta(1f / 30f);
        var t = clock.Step(2);
        await Assert.That(t).IsEqualTo(2f / 30f);
        await Assert.That(clock.Time).IsEqualTo(2f / 30f);
    }

    [Test]
    public async Task Reset_clears_accumulated_time()
    {
        var clock = new DeterministicFrameClock();
        clock.Step();
        clock.Reset();
        await Assert.That(clock.Time).IsEqualTo(0f);
    }
}

public sealed class SimulatedInputTests
{
    [Test]
    public async Task Press_dequeues_in_order()
    {
        var input = new SimulatedInput();
        input.Press(KeyboardKey.Space);
        input.Press(KeyboardKey.Escape);
        await Assert.That(input.TryDequeue(out var first)).IsTrue();
        await Assert.That(first).IsEqualTo(KeyboardKey.Space);
        await Assert.That(input.TryDequeue(out var second)).IsTrue();
        await Assert.That(second).IsEqualTo(KeyboardKey.Escape);
        await Assert.That(input.TryDequeue(out _)).IsFalse();
    }

    [Test]
    public async Task Clear_empties_queue()
    {
        var input = new SimulatedInput();
        input.Press(KeyboardKey.A);
        input.Clear();
        await Assert.That(input.TryDequeue(out _)).IsFalse();
    }
}

public sealed class DelegateRaylibFrameRendererTests
{
    [Test]
    public async Task OnFrame_invokes_delegate()
    {
        float seenDt = 0;
        var seenW = 0;
        var seenH = 0;
        var renderer = new DelegateRaylibFrameRenderer((dt, w, h) =>
        {
            seenDt = dt;
            seenW = w;
            seenH = h;
        });
        renderer.OnFrame(0.016f, 800, 600);
        await Assert.That(seenDt).IsEqualTo(0.016f);
        await Assert.That(seenW).IsEqualTo(800);
        await Assert.That(seenH).IsEqualTo(600);
    }
}

public sealed class GoldenAdhocRunBucketLayoutTests
{
    [Test]
    public async Task Resolve_creates_story_directory_under_shared_run()
    {
        GoldenAdhocRunBucketLayout.ResetSharedRun();
        var tempRoot = Path.Combine(Path.GetTempPath(), "novolis-testing-unit", Guid.NewGuid().ToString("N"));
        try
        {
            var ctx = GoldenAdhocRunBucketLayout.Instance.Resolve(typeof(GoldenAdhocRunBucketLayoutTests).Assembly, "story-a", tempRoot);
            await Assert.That(Directory.Exists(ctx.StoryDirectory)).IsTrue();
            await Assert.That(ctx.StoryId).IsEqualTo("story-a");
            await Assert.That(GoldenAdhocRunBucketLayout.SharedRunFolder).IsNotNull();
            var ctx2 = GoldenAdhocRunBucketLayout.Instance.Resolve(typeof(GoldenAdhocRunBucketLayoutTests).Assembly, "story-b", tempRoot);
            await Assert.That(ctx2.RunFolder).IsEqualTo(ctx.RunFolder);
        }
        finally
        {
            GoldenAdhocRunBucketLayout.ResetSharedRun();
            if (Directory.Exists(tempRoot))
                Directory.Delete(tempRoot, recursive: true);
        }
    }
}

public sealed class GoldenRenderOutputLayoutTests
{
    [Test]
    public async Task ResetSharedRun_clears_bucket_state()
    {
        GoldenRenderOutputLayout.ResetSharedRun();
        await Assert.That(GoldenRenderOutputLayout.SharedRunFolder).IsNull();
    }
}

public sealed class GoldenTestPollingTests
{
    [Test]
    public async Task WaitUntil_returns_when_predicate_true()
    {
        var count = 0;
        GoldenTestPolling.WaitUntil(() => ++count >= 2, TimeSpan.FromSeconds(2), TimeSpan.FromMilliseconds(10));
        await Assert.That(count).IsGreaterThanOrEqualTo(2);
    }

    [Test]
    public async Task WaitUntilAsync_returns_when_predicate_true()
    {
        var ready = false;
        _ = Task.Run(async () =>
        {
            await Task.Delay(50);
            ready = true;
        });
        await GoldenTestPolling.WaitUntilAsync(() => ready, TimeSpan.FromSeconds(2), TimeSpan.FromMilliseconds(10));
        await Assert.That(ready).IsTrue();
    }
}
