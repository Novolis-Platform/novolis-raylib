namespace Novolis.Raylib.Testing.Golden;

/// <summary>Blocking wait helpers for golden tests and hosted game setup.</summary>
public static class GoldenTestPolling
{
    public static void WaitUntil(
        Func<bool> predicate,
        TimeSpan timeout,
        TimeSpan? pollInterval = null)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        var interval = pollInterval ?? TimeSpan.FromMilliseconds(100);
        var deadline = DateTime.UtcNow.Add(timeout);
        while (DateTime.UtcNow < deadline)
        {
            if (predicate())
                return;

            Thread.Sleep(interval);
        }
    }

    public static async Task WaitUntilAsync(
        Func<bool> predicate,
        TimeSpan timeout,
        TimeSpan? pollInterval = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        var interval = pollInterval ?? TimeSpan.FromMilliseconds(100);
        var deadline = DateTime.UtcNow.Add(timeout);
        while (DateTime.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (predicate())
                return;

            await Task.Delay(interval, cancellationToken).ConfigureAwait(false);
        }
    }
}
