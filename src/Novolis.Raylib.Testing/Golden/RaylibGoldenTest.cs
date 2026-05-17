using System.Reflection;
using Novolis.Raylib.Abstractions;
using Novolis.Raylib.Capture;

namespace Novolis.Raylib.Testing.Golden;

/// <summary>Golden image tests with QA review bundles and SHA256 validation (no environment variables).</summary>
public static class RaylibGoldenTest
{
    public static GoldenTestResult Run(
        string storyId,
        IRaylibFrameRenderer renderer,
        GoldenRunOptions? options = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storyId);
        ArgumentNullException.ThrowIfNull(renderer);
        options ??= new GoldenRunOptions();

        var assembly = options.TestAssembly ?? System.Reflection.Assembly.GetCallingAssembly();
        var goldensRoot = options.GoldensRoot;
        GoldenStorySpec spec;
        try
        {
            spec = GoldenCatalog.LoadStory(assembly, storyId, goldensRoot);
        }
        catch (Exception ex)
        {
            return GoldenTestResult.Fail($"Failed to load golden spec: {ex.Message}");
        }

        var renderContext = GoldenRenderOutputLayout.Resolve(assembly, storyId, options.OutputRoot);
        using var testScope = RaylibTestRuntime.EnterNativeOffscreen();

        using var captureSession = options.EnableStreamingCapture
            ? new FrameCaptureSession(new CaptureStreamOptions
            {
                CaptureEveryNFrames = options.CaptureEveryNFrames,
                MaxBufferedFrames = options.MaxBufferedFrames,
            })
            : null;

        if (!RaylibOffscreenTestHarness.IsNativeOffscreenRunRequested())
        {
            var skipReason = "Native offscreen not enabled. Call RaylibTestRuntime.EnableForAssembly() or EnterNativeOffscreen().";
            var reportPath = GoldenRenderReportWriter.WriteSkippedReport(renderContext, spec, skipReason);
            Console.WriteLine($"Golden render report: {reportPath}");
            return GoldenTestResult.Skip(skipReason, reportPath);
        }

        var harnessResult = RaylibOffscreenTestHarness.Run(
            renderer,
            new RaylibOffscreenTestOptions
            {
                WindowTitle = $"Golden.{spec.StoryId}",
                Width = spec.Width,
                Height = spec.Height,
                MaxFrames = spec.MaxFrames,
                HideWindow = true,
                CaptureLastFramePng = true,
            });

        if (!harnessResult.RanNativeLoop)
        {
            var skipMsg = harnessResult.Message ?? "Native offscreen run skipped.";
            var reportPath = GoldenRenderReportWriter.WriteSkippedReport(renderContext, spec, skipMsg);
            Console.WriteLine($"Golden render report: {reportPath}");
            return GoldenTestResult.Skip(skipMsg, reportPath);
        }

        if (!harnessResult.Succeeded || harnessResult.LastFramePng is null)
            return GoldenTestResult.Fail(harnessResult.Message ?? "Harness failed.", storyDirectory: renderContext.StoryDirectory);

        var actualPng = harnessResult.LastFramePng;
        var actualSha = FramebufferAssert.Sha256Hex(actualPng);
        var baselinePath = GoldenCatalog.GetBaselinePngPath(assembly, storyId, goldensRoot);
        var baselinePng = File.Exists(baselinePath) ? File.ReadAllBytes(baselinePath) : [];
        var baselineSha = baselinePng.Length > 0 ? FramebufferAssert.Sha256Hex(baselinePng) : spec.BaselineSha256;

        bool? assertPassed = null;
        string? assertError = null;

        try
        {
            if (options.Mode == GoldenRunMode.UpdateBaselines)
            {
                var committedDir = GoldenCatalog.GetStoryDirectory(assembly, storyId, goldensRoot);
                Directory.CreateDirectory(committedDir);
                File.WriteAllBytes(baselinePath, actualPng);
                spec.BaselineSha256 = actualSha;
                spec.SaveToFile(Path.Combine(committedDir, "spec.json"));
                assertPassed = true;
            }
            else if (options.Mode == GoldenRunMode.Assert)
            {
                var expected = string.IsNullOrWhiteSpace(spec.BaselineSha256) ? baselineSha : spec.BaselineSha256;
                if (string.IsNullOrWhiteSpace(expected))
                {
                    throw new InvalidOperationException(
                        $"No baseline for '{storyId}'. Run UpdateBaselinesTests or GoldenRunMode.UpdateBaselines.");
                }

                FramebufferAssert.AssertHash(actualPng, expected);
                assertPassed = true;
            }
        }
        catch (Exception ex)
        {
            assertPassed = false;
            assertError = ex.Message;
        }

        var assertInfo = new GoldenRenderReportWriter.AssertInfo
        {
            AssertPassed = assertPassed,
            ActualSha256 = actualSha,
            BaselineSha256 = baselineSha,
            ErrorMessage = assertError,
        };

        if (options.Mode == GoldenRunMode.UpdateBaselines && baselinePng.Length == 0)
            baselinePng = actualPng;

        var reviewPath = GoldenRenderReportWriter.Write(renderContext, spec, actualPng, baselinePng, assertInfo);
        Console.WriteLine($"Golden render report: {reviewPath}");

        if (assertPassed == false)
            return GoldenTestResult.Fail(assertError ?? "Golden assert failed", reviewPath, renderContext.StoryDirectory);

        return GoldenTestResult.Pass(reviewPath, renderContext.StoryDirectory, actualPng, assertPassed ?? true);
    }
}
