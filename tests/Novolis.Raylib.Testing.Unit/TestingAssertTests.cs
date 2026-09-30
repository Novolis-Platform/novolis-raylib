using System.Reflection;
using System.Text;
using Novolis.Raylib.Testing;
using Novolis.Raylib.Testing.Golden;

namespace Novolis.Raylib.Testing.Unit;

public sealed class FramebufferAssertTests
{
    [Test]
    public async Task Sha256Hex_is_lowercase_and_stable()
    {
        var bytes = Encoding.UTF8.GetBytes("png-payload");
        var hex = FramebufferAssert.Sha256Hex(bytes);
        await Assert.That(hex).IsEqualTo(hex.ToLowerInvariant());
        await Assert.That(hex.Length).IsEqualTo(64);
        await Assert.That(FramebufferAssert.Sha256Hex(bytes)).IsEqualTo(hex);
    }

    [Test]
    public async Task AssertHash_passes_for_matching_digest()
    {
        var bytes = Encoding.UTF8.GetBytes("match-me");
        var hex = FramebufferAssert.Sha256Hex(bytes);
        FramebufferAssert.AssertHash(bytes, hex);
        await Task.CompletedTask;
    }

    [Test]
    public async Task AssertHash_throws_on_mismatch()
    {
        var bytes = Encoding.UTF8.GetBytes("png");
        var threw = false;
        try
        {
            FramebufferAssert.AssertHash(bytes, new string('a', 64));
        }
        catch (InvalidOperationException ex)
        {
            threw = true;
            await Assert.That(ex.Message).Contains("hash mismatch");
        }

        await Assert.That(threw).IsTrue();
    }

    [Test]
    public async Task AssertMatchesBaseline_uses_embedded_sha256()
    {
        var png = Encoding.UTF8.GetBytes("embedded-baseline");
        var sha = FramebufferAssert.Sha256Hex(png);
        var spec = new GoldenStorySpec
        {
            StoryId = "embedded",
            BaselineSha256 = sha,
        };
        var frame = new GoldenFrameSpec { FrameId = GoldenFrameSpec.DefaultFrameId, BaselineSha256 = sha };

        FramebufferAssert.AssertMatchesBaseline(png, spec, frame, typeof(FramebufferAssertTests).Assembly);
        await Task.CompletedTask;
    }
}
