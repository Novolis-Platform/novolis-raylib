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

public sealed class GoldenCatalogExtendedTests
{
    [Test]
    public async Task GetGoldensRoot_honors_explicit_override()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "novolis-testing-unit", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot);
        try
        {
            var resolved = GoldenCatalog.GetGoldensRoot(typeof(GoldenCatalogExtendedTests).Assembly, tempRoot);
            await Assert.That(resolved).IsEqualTo(Path.GetFullPath(tempRoot));
        }
        finally
        {
            Directory.Delete(tempRoot, recursive: true);
        }
    }

    [Test]
    public async Task GetBaselinePngPath_uses_frame_id_for_multiframe_stories()
    {
        var assembly = typeof(GoldenCatalogExtendedTests).Assembly;
        var tempRoot = Path.Combine(Path.GetTempPath(), "novolis-testing-unit", Guid.NewGuid().ToString("N"));
        try
        {
            var path = GoldenCatalog.GetBaselinePngPath(assembly, "story", "frame-2", tempRoot);
            await Assert.That(path).EndsWith(Path.Combine("story", "frame-2.png"));
        }
        finally
        {
            if (Directory.Exists(tempRoot))
                Directory.Delete(tempRoot, recursive: true);
        }
    }

    [Test]
    public async Task GetStoryDirectory_combines_root_and_story_id()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "novolis-testing-unit", Guid.NewGuid().ToString("N"));
        try
        {
            var dir = GoldenCatalog.GetStoryDirectory(typeof(GoldenCatalogExtendedTests).Assembly, "demo", tempRoot);
            await Assert.That(dir).EndsWith(Path.Combine("demo"));
        }
        finally
        {
            if (Directory.Exists(tempRoot))
                Directory.Delete(tempRoot, recursive: true);
        }
    }

    [Test]
    public async Task LoadStory_reads_spec_from_temp_directory()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "novolis-testing-unit", Guid.NewGuid().ToString("N"));
        var storyDir = Path.Combine(tempRoot, "demo-story");
        Directory.CreateDirectory(storyDir);
        File.WriteAllText(
            Path.Combine(storyDir, "spec.json"),
            """
            {
              "schemaVersion": 2,
              "storyId": "demo-story",
              "title": "Demo",
              "baselineSha256": "abc"
            }
            """);

        try
        {
            var spec = GoldenCatalog.LoadStory(typeof(GoldenCatalogExtendedTests).Assembly, "demo-story", tempRoot);
            await Assert.That(spec.StoryId).IsEqualTo("demo-story");
            await Assert.That(spec.Title).IsEqualTo("Demo");
        }
        finally
        {
            Directory.Delete(tempRoot, recursive: true);
        }
    }
}

public sealed class GoldenPublishResultTests
{
    [Test]
    public async Task IndexHtmlUri_is_file_scheme()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "novolis-testing-unit", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        var indexPath = Path.Combine(tempDir, "index.html");
        File.WriteAllText(indexPath, "<html></html>");

        try
        {
            var result = new GoldenPublishResult
            {
                DestinationDirectory = tempDir,
                IndexHtmlPath = indexPath,
            };
            await Assert.That(result.IndexHtmlUri).StartsWith("file:");
            await Assert.That(result.IndexHtmlUri).Contains("index.html");
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }
}

public sealed class VisualCaptureArtifactsExtendedTests
{
    [Test]
    public async Task WritePng_throws_on_blank_file_name()
    {
        var threw = false;
        try
        {
#pragma warning disable CS0618
            VisualCaptureArtifacts.WritePng([], " ");
#pragma warning restore CS0618
        }
        catch (ArgumentException)
        {
            threw = true;
        }

        await Assert.That(threw).IsTrue();
    }

    [Test]
    public async Task RelativeCapturesDir_points_at_artifacts_folder()
    {
        await Assert.That(VisualCaptureArtifacts.RelativeCapturesDir)
            .IsEqualTo("artifacts/visual-captures");
    }
}
