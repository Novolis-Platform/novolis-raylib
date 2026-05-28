using Novolis.Raylib.Internal;

namespace Novolis.Raylib.Testing;

/// <summary>
/// Serializes GLFW / raylib window initialization across parallel test hosts (Rider, MTP).
/// Delegates to <see cref="RaylibGlfwProcessSync"/>.
/// </summary>
public static class RaylibGlfwTestSync
{
    /// <summary>Acquires the global GLFW mutex (blocks up to two minutes).</summary>
    public static RaylibGlfwProcessSync.LockScope Enter() => RaylibGlfwProcessSync.Enter();
}
