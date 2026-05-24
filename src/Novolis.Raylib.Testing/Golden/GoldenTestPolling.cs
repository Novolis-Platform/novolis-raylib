namespace Novolis.Raylib.Testing.Golden;

/// <summary>Blocking wait helpers for golden tests and hosted game setup.</summary>
public static class GoldenTestPolling
{
    /// <summary>Polls until <paramref name="predicate"/> returns true or timeout elapses.</summary>
    /// <param name="predicate">Condition to satisfy.</param>
    /// <param name="timeout">Maximum wait duration.</param>
    /// <param name="pollInterval">Delay between polls (default 100 ms).</param>
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

    /// <summary>Async poll until <paramref name="predicate"/> returns true or timeout elapses.</summary>
    /// <param name="predicate">Condition to satisfy.</param>
    /// <param name="timeout">Maximum wait duration.</param>
    /// <param name="pollInterval">Delay between polls (default 100 ms).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task that completes when the predicate succeeds or times out.</returns>
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
