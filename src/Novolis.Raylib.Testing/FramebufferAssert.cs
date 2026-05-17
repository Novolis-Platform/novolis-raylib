using System.Reflection;
using System.Security.Cryptography;

namespace Novolis.Raylib.Testing;

/// <summary>PNG/hash assertions for offscreen captures (opt-in golden workflows).</summary>
public static class FramebufferAssert
{
    public static string Sha256Hex(ReadOnlySpan<byte> pngBytes)
    {
        var hash = SHA256.HashData(pngBytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public static void AssertHash(ReadOnlySpan<byte> pngBytes, string expectedSha256Hex)
    {
        var actual = Sha256Hex(pngBytes);
        if (!actual.Equals(expectedSha256Hex, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"PNG hash mismatch. Expected {expectedSha256Hex}, got {actual}.");
    }

    public static void AssertMatchesBaseline(ReadOnlySpan<byte> pngBytes, Golden.GoldenStorySpec spec, Assembly testAssembly)
    {
        ArgumentNullException.ThrowIfNull(spec);
        if (!string.IsNullOrWhiteSpace(spec.BaselineSha256))
        {
            AssertHash(pngBytes, spec.BaselineSha256);
            return;
        }

        var baselinePath = Golden.GoldenCatalog.GetBaselinePngPath(testAssembly, spec.StoryId);
        if (!File.Exists(baselinePath))
            throw new FileNotFoundException($"Baseline PNG not found: {baselinePath}", baselinePath);

        AssertHash(pngBytes, Sha256Hex(File.ReadAllBytes(baselinePath)));
    }
}
