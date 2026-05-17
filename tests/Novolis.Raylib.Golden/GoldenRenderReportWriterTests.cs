using Novolis.Raylib.Testing.Golden;

namespace Novolis.Raylib.Golden;

public sealed class GoldenRenderReportWriterTests
{
    [Test]
    public async Task BuildIndexHtml_single_frame_uses_legacy_png_names()
    {
        var dir = CreateTempDir();
        try
        {
            await File.WriteAllBytesAsync(Path.Combine(dir, "actual.png"), MinimalPng);

            var spec = new GoldenStorySpec
            {
                StoryId = "test-story",
                Title = "Test Story",
                Expectations = ["First expectation.", "Second expectation."],
            };

            var context = CreateContext(dir);
            var frames = new[]
            {
                new GoldenFrameCaptureResult
                {
                    Frame = spec.GetEffectiveFrames()[0],
                    ActualPng = MinimalPng,
                    AssertPassed = true,
                    ActualSha256 = "abc123",
                },
            };

            var html = GoldenRenderReportWriter.BuildIndexHtml(
                context,
                spec,
                frames,
                new GoldenStoryAssertInfo { AssertPassed = true },
                dir);

            await Assert.That(html).Contains("--bg:#0b0d11");
            await Assert.That(html).Contains("class=\"dash\"");
            await Assert.That(html).Contains("class=\"grp ");
            await Assert.That(html).Contains("src=\"actual.png\"");
            await Assert.That(html).Contains("First expectation.");
            await Assert.That(html).Contains("class=\"t-badge passed\"");
        }
        finally
        {
            TryDeleteDir(dir);
        }
    }

    [Test]
    public async Task BuildIndexHtml_multi_frame_emits_sections_and_missing_placeholder()
    {
        var dir = CreateTempDir();
        try
        {
            await File.WriteAllBytesAsync(Path.Combine(dir, "01-a.actual.png"), MinimalPng);

            var spec = new GoldenStorySpec
            {
                StoryId = "multi-story",
                Title = "Multi Story",
                Frames =
                [
                    new GoldenFrameSpec
                    {
                        FrameId = "01-a",
                        Title = "Alpha",
                        Caption = "First step.",
                        Expectations = ["Alpha checklist item."],
                    },
                    new GoldenFrameSpec
                    {
                        FrameId = "02-b",
                        Title = "Beta",
                        Caption = "Second step.",
                        Expectations = ["Beta checklist item."],
                    },
                ],
            };

            var context = CreateContext(dir);
            var frames = new[]
            {
                new GoldenFrameCaptureResult
                {
                    Frame = spec.Frames[0],
                    ActualPng = MinimalPng,
                    AssertPassed = true,
                },
                new GoldenFrameCaptureResult
                {
                    Frame = spec.Frames[1],
                    Skipped = true,
                    SkipReason = "native unavailable",
                },
            };

            var html = GoldenRenderReportWriter.BuildIndexHtml(
                context,
                spec,
                frames,
                new GoldenStoryAssertInfo { AssertPassed = false },
                dir);

            await Assert.That(html).Contains("1 — Alpha");
            await Assert.That(html).Contains("2 — Beta");
            await Assert.That(html).Contains("src=\"01-a.actual.png\"");
            await Assert.That(html).Contains("Alpha checklist item.");
            await Assert.That(html).Contains("Beta checklist item.");
            await Assert.That(html).Contains("class=\"golden-missing\"");
            await Assert.That(html).Contains("<strong>FAIL</strong>");
            await Assert.That(html).Contains("class=\"t-badge skipped\"");
        }
        finally
        {
            TryDeleteDir(dir);
        }
    }

    [Test]
    public async Task BuildIndexHtml_fail_frame_uses_fail_status_class()
    {
        var dir = CreateTempDir();
        try
        {
            var spec = new GoldenStorySpec
            {
                StoryId = "fail-story",
                Title = "Fail Story",
                Expectations = ["Check."],
            };

            var context = CreateContext(dir);
            var frames = new[]
            {
                new GoldenFrameCaptureResult
                {
                    Frame = spec.GetEffectiveFrames()[0],
                    AssertPassed = false,
                    ErrorMessage = "hash mismatch",
                },
            };

            var html = GoldenRenderReportWriter.BuildIndexHtml(
                context,
                spec,
                frames,
                new GoldenStoryAssertInfo { AssertPassed = false, ErrorMessage = "hash mismatch" },
                dir);

            await Assert.That(html).Contains("class=\"t-badge failed\"");
            await Assert.That(html).Contains("hash mismatch");
        }
        finally
        {
            TryDeleteDir(dir);
        }
    }

    private static GoldenRenderRunContext CreateContext(string storyDirectory)
    {
        var runFolder = Path.Combine(Path.GetTempPath(), "golden-run-" + Guid.NewGuid().ToString("N"));
        return new GoldenRenderRunContext
        {
            RunFolder = runFolder,
            StoryDirectory = storyDirectory,
            AssemblySegment = "Test_Assembly",
            StoryId = "test-story",
        };
    }

    private static string CreateTempDir()
    {
        var dir = Path.Combine(Path.GetTempPath(), "golden-html-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return dir;
    }

    private static void TryDeleteDir(string dir)
    {
        try
        {
            Directory.Delete(dir, recursive: true);
        }
        catch
        {
            /* best-effort */
        }
    }

    private static readonly byte[] MinimalPng = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
}
