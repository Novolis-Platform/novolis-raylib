using System.Reflection;
using Novolis.Raylib.Diagnostics;

namespace Novolis.Raylib.Testing.Golden;

/// <summary>Options for <see cref="RaylibGoldenTest.Run"/> (no environment variables).</summary>
public sealed class GoldenRunOptions
{
    public GoldenRunMode Mode { get; init; } = GoldenRunMode.Assert;

    public string? OutputRoot { get; init; }

    public bool EnableStreamingCapture { get; init; }

    public Assembly? TestAssembly { get; init; }

    /// <summary>Override committed <c>Goldens/</c> root (for baseline seed console).</summary>
    public string? GoldensRoot { get; init; }
}
