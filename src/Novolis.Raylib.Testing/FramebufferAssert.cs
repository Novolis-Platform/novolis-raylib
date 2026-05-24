using System.Reflection;
using System.Security.Cryptography;

namespace Novolis.Raylib.Testing;

/// <summary>PNG/hash assertions for offscreen captures (opt-in golden workflows).</summary>
public static class FramebufferAssert
{
    /// <summary>Computes lowercase hex SHA-256 of PNG bytes.</summary>
    /// <param name="pngBytes">PNG file contents.</param>
    /// <returns>Lowercase hex digest.</returns>
    public static string Sha256Hex(ReadOnlySpan<byte> pngBytes)
    {
        var hash = SHA256.HashData(pngBytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    /// <summary>Throws when PNG hash does not match <paramref name="expectedSha256Hex"/>.</summary>
    /// <param name="pngBytes">PNG file contents.</param>
    /// <param name="expectedSha256Hex">Expected lowercase hex SHA-256.</param>
    public static void AssertHash(ReadOnlySpan<byte> pngBytes, string expectedSha256Hex)
    {
        var actual = Sha256Hex(pngBytes);
        if (!actual.Equals(expectedSha256Hex, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"PNG hash mismatch. Expected {expectedSha256Hex}, got {actual}.");
    }

    /// <summary>Asserts PNG matches the story's first effective frame baseline.</summary>
    /// <param name="pngBytes">Captured PNG bytes.</param>
    /// <param name="spec">Golden story specification.</param>
    /// <param name="testAssembly">Test assembly for baseline path resolution.</param>
    public static void AssertMatchesBaseline(ReadOnlySpan<byte> pngBytes, Golden.GoldenStorySpec spec, Assembly testAssembly) =>
        AssertMatchesBaseline(pngBytes, spec, spec.GetEffectiveFrames()[0], testAssembly);

    /// <summary>Asserts PNG matches embedded hash or committed baseline PNG for one frame.</summary>
    /// <param name="pngBytes">Captured PNG bytes.</param>
    /// <param name="spec">Golden story specification.</param>
    /// <param name="frame">Frame specification to validate.</param>
    /// <param name="testAssembly">Test assembly for baseline path resolution.</param>
    /// <param name="goldensRoot">Optional override for committed <c>Goldens/</c> root.</param>
    public static void AssertMatchesBaseline(
        ReadOnlySpan<byte> pngBytes,
        Golden.GoldenStorySpec spec,
        Golden.GoldenFrameSpec frame,
        Assembly testAssembly,
        string? goldensRoot = null)
    {
        ArgumentNullException.ThrowIfNull(spec);
        ArgumentNullException.ThrowIfNull(frame);

        if (!string.IsNullOrWhiteSpace(frame.BaselineSha256))
        {
            AssertHash(pngBytes, frame.BaselineSha256);
            return;
        }

        if (!spec.IsMultiFrame && !string.IsNullOrWhiteSpace(spec.BaselineSha256))
        {
            AssertHash(pngBytes, spec.BaselineSha256);
            return;
        }

        var baselinePath = Golden.GoldenCatalog.GetBaselinePngPath(
            testAssembly,
            spec.StoryId,
            spec.IsMultiFrame ? frame.FrameId : null,
            goldensRoot);
        if (!File.Exists(baselinePath))
            throw new FileNotFoundException($"Baseline PNG not found: {baselinePath}", baselinePath);

        AssertHash(pngBytes, Sha256Hex(File.ReadAllBytes(baselinePath)));
    }
}
