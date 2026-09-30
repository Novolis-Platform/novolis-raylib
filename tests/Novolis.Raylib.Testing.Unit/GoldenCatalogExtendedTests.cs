using System.Reflection;
using System.Text;
using Novolis.Raylib.Testing;
using Novolis.Raylib.Testing.Golden;

namespace Novolis.Raylib.Testing.Unit;

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
