using System.Diagnostics;
using Novolis.Raylib.Internal;
using Novolis.Raylib.Shell;

namespace Novolis.Raylib.Runtime.Unit;

[NotInParallel("raylib-glfw-lock")]
public sealed class RaylibGlfwProcessSyncTests
{
    [Test]
    public async Task Enter_acquires_and_releases_mutex()
    {
        using var scope = RaylibGlfwProcessSync.Enter();
        await Assert.That(scope.GetType().Name).IsEqualTo("LockScope");
    }

    [Test]
    public async Task TryEnter_fails_fast_while_held()
    {
        using var held = HoldLockOnBackgroundThread();
        var clock = Stopwatch.StartNew();
        var acquired = RaylibGlfwProcessSync.TryEnter(TimeSpan.FromMilliseconds(80), out var blocked);
        clock.Stop();

        await Assert.That(acquired).IsFalse();
        await Assert.That(clock.Elapsed).IsLessThan(TimeSpan.FromSeconds(2));
        blocked.Dispose();
    }

    [Test]
    public async Task Enter_timeout_throws_without_waiting_default_two_minutes()
    {
        using var held = HoldLockOnBackgroundThread();
        var clock = Stopwatch.StartNew();
        var threw = false;
        try
        {
            RaylibGlfwProcessSync.Enter(TimeSpan.FromMilliseconds(50));
        }
        catch (InvalidOperationException ex)
        {
            threw = ex.Message.Contains("Raylib GLFW lock", StringComparison.Ordinal);
        }

        await Assert.That(threw).IsTrue();
        await Assert.That(clock.Elapsed).IsLessThan(TimeSpan.FromSeconds(2));
    }

    private static IDisposable HoldLockOnBackgroundThread()
    {
        var started = new ManualResetEventSlim(false);
        var release = new ManualResetEventSlim(false);
        Exception? fault = null;
        var holder = new Thread(() =>
        {
            try
            {
                using var scope = RaylibGlfwProcessSync.Enter();
                started.Set();
                release.Wait();
            }
            catch (Exception ex)
            {
                fault = ex;
                started.Set();
            }
        })
        {
            IsBackground = true,
            Name = "RaylibGlfwTestHolder",
        };
        holder.Start();
        if (!started.Wait(TimeSpan.FromSeconds(2)))
            throw new TimeoutException("Background thread did not acquire the GLFW lock.");
        if (fault is not null)
            throw new InvalidOperationException("Background thread failed to acquire the GLFW lock.", fault);

        return new BackgroundLock(release, holder);
    }

    private sealed class BackgroundLock(ManualResetEventSlim release, Thread holder) : IDisposable
    {
        public void Dispose()
        {
            release.Set();
            holder.Join(TimeSpan.FromSeconds(2));
            release.Dispose();
        }
    }

    [Test]
    public async Task TryEnter_succeeds_after_release()
    {
        RaylibGlfwProcessSync.Enter().Dispose();
        var acquired = RaylibGlfwProcessSync.TryEnter(TimeSpan.FromSeconds(2), out var scope);
        using (scope)
            await Assert.That(acquired).IsTrue();
    }
}
