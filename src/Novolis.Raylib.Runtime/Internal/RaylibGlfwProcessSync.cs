namespace Novolis.Raylib.Internal;

/// <summary>
/// Serializes GLFW / raylib window initialization across processes and threads.
/// GLFW is not safe when multiple callers invoke <c>InitWindow</c> concurrently.
/// </summary>
public static class RaylibGlfwProcessSync
{
    private const string MutexName = @"Global\Novolis.Raylib.Glfw";

    /// <summary>Acquires the global GLFW mutex (blocks up to two minutes).</summary>
    public static LockScope Enter()
    {
        var mutex = new Mutex(initiallyOwned: false, name: MutexName);
        try
        {
            if (!mutex.WaitOne(TimeSpan.FromMinutes(2)))
            {
                throw new InvalidOperationException("Timed out waiting for the Raylib GLFW lock.");
            }
        }
        catch
        {
            mutex.Dispose();
            throw;
        }

        return new LockScope(mutex);
    }

    /// <summary>RAII holder for the GLFW process mutex.</summary>
    public readonly struct LockScope(Mutex mutex) : IDisposable
    {
        /// <inheritdoc />
        public void Dispose()
        {
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
