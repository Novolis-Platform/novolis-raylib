using Novolis.Raylib.Testing.Golden;

namespace Novolis.Raylib.Golden;

public sealed class GoldenArtifactPublisherTests
{
    private static readonly byte[] MinimalPng = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    [Test]
    public async Task Publish_copies_index_and_stable_png_names()
    {
        var source = CreateTempDir();
        var dest = CreateTempDir();
        try
        {
            await File.WriteAllTextAsync(
                Path.Combine(source, "index.html"),
                "<html><body><img src=\"01-a.actual.png\" /></body></html>");
            await File.WriteAllBytesAsync(Path.Combine(source, "01-a.actual.png"), MinimalPng);
            await File.WriteAllBytesAsync(Path.Combine(source, "02-b.actual.png"), MinimalPng);
            await File.WriteAllTextAsync(Path.Combine(source, "manifest.json"), "{}");

            var result = GoldenArtifactPublisher.Publish(
                source,
                dest,
                new GoldenPublishOptions { ReadmeStepSummary = "01-a → 02-b" });

            await Assert.That(File.Exists(result.IndexHtmlPath)).IsTrue();
            await Assert.That(File.Exists(Path.Combine(dest, "01-a.png"))).IsTrue();
            await Assert.That(File.Exists(Path.Combine(dest, "02-b.png"))).IsTrue();
            await Assert.That(File.Exists(Path.Combine(dest, "manifest.json"))).IsTrue();
            await Assert.That(result.CopiedPngPaths.Count).IsEqualTo(2);
            var readme = await File.ReadAllTextAsync(Path.Combine(dest, "README.txt"));
            await Assert.That(readme).Contains("01-a → 02-b");
        }
        finally
        {
            TryDeleteDir(source);
            TryDeleteDir(dest);
        }
    }

    [Test]
    public async Task Publish_writes_fallback_index_when_source_missing_html()
    {
        var source = CreateTempDir();
        var dest = CreateTempDir();
        try
        {
            var result = GoldenArtifactPublisher.Publish(
                source,
                dest,
                new GoldenPublishOptions
                {
                    SkippedMessage = "native unavailable",
                    FallbackTitle = "Test story",
                });

            var html = await File.ReadAllTextAsync(result.IndexHtmlPath);
            await Assert.That(html).Contains("native unavailable");
            await Assert.That(html).Contains("Test story");
        }
        finally
        {
            TryDeleteDir(source);
            TryDeleteDir(dest);
        }
    }

    private static string CreateTempDir()
    {
        var dir = Path.Combine(Path.GetTempPath(), "novolis-golden-publish-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return dir;
    }

    private static void TryDeleteDir(string dir)
    {
        try
        {
            if (Directory.Exists(dir))
                Directory.Delete(dir, recursive: true);
        }
        catch
        {
            /* best-effort */
        }
    }
}
