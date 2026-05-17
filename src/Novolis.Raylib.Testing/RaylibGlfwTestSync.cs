namespace Novolis.Raylib.Testing;

/// <summary>
/// Serializes GLFW / raylib window initialization across parallel test hosts (Rider, MTP).
/// GLFW is not safe when multiple processes or threads call <c>InitWindow</c> concurrently.
/// </summary>
public static class RaylibGlfwTestSync
{
    private const string MutexName = @"Global\Novolis.Raylib.GlfwTests";

    public static LockScope Enter()
    {
        var mutex = new Mutex(initiallyOwned: false, name: MutexName);
        try
        {
            if (!mutex.WaitOne(TimeSpan.FromMinutes(2)))
                throw new InvalidOperationException("Timed out waiting for the GLFW test lock.");
        }
        catch
        {
            mutex.Dispose();
            throw;
        }

        return new LockScope(mutex);
    }

    public readonly struct LockScope(Mutex mutex) : IDisposable
    {
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
