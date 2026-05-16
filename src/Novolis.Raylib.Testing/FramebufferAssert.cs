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
}
