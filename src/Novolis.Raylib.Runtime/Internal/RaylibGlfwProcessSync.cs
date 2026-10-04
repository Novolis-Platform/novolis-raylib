namespace Novolis.Raylib.Internal;

/// <summary>
/// Serializes GLFW / raylib window initialization across processes and threads.
/// GLFW is not safe when multiple callers invoke <c>InitWindow</c> concurrently.
/// </summary>
public static class RaylibGlfwProcessSync
{
    private const string MutexName = @"Global\Novolis.Raylib.Glfw";

    /// <summary>Wait used by standalone shells that may contend with another process during startup.</summary>
    public static readonly TimeSpan DefaultTimeout = TimeSpan.FromMinutes(2);

    /// <summary>Wait used by hidden UI hosts. Fail fast instead of hanging the Avalonia process.</summary>
    public static readonly TimeSpan EmbeddedHostTimeout = TimeSpan.FromSeconds(5);

    /// <summary>Acquires the global GLFW mutex (blocks up to <see cref="DefaultTimeout"/>).</summary>
    public static LockScope Enter() => Enter(DefaultTimeout);

    /// <summary>Acquires the global GLFW mutex, or throws if the wait expires.</summary>
    /// <remarks>
    /// <see cref="AbandonedMutexException"/> means a previous owner exited without
    /// <c>ReleaseMutex</c>; the wait still grants ownership — treat as success.
    /// </remarks>
    public static LockScope Enter(TimeSpan timeout)
    {
        if (!TryEnter(timeout, out var scope))
            throw new InvalidOperationException("Timed out waiting for the Raylib GLFW lock.");

        return scope;
    }

    /// <summary>Attempts to acquire the global GLFW mutex without throwing on timeout.</summary>
    public static bool TryEnter(TimeSpan timeout, out LockScope scope)
    {
        var mutex = new Mutex(initiallyOwned: false, name: MutexName);
        try
        {
            try
            {
                if (!mutex.WaitOne(timeout))
                {
                    mutex.Dispose();
                    scope = default;
                    return false;
                }
            }
            catch (AbandonedMutexException)
            {
                // Previous process/thread died holding the lock; we now own it.
            }
        }
        catch
        {
            mutex.Dispose();
            throw;
        }

        scope = new LockScope(mutex);
        return true;
    }

    /// <summary>RAII holder for the GLFW process mutex.</summary>
    public readonly struct LockScope(Mutex mutex) : IDisposable
    {
        /// <inheritdoc />
        public void Dispose()
        {
            if (mutex is null)
                return;

            try
            {
                mutex.ReleaseMutex();
            }
            catch (ApplicationException)
            {
                // Current process does not own the mutex.
            }

            mutex.Dispose();
        }
    }
}
