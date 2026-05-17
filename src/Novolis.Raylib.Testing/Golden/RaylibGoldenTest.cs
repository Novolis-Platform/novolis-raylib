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
        GoldenRunOptions? options = null) =>
        Run(storyId, new GoldenStoryRendererAdapter(renderer), options);

    public static GoldenTestResult Run(
        string storyId,
        IGoldenStoryRenderer renderer,
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

        var frameSpecs = spec.GetEffectiveFrames();
        var frameResults = new List<GoldenFrameCaptureResult>();
        byte[]? lastActualPng = null;
        var storySkipped = false;
        string? storySkipReason = null;

        foreach (var frameSpec in frameSpecs)
        {
            renderer.BeginFrame(frameSpec.FrameId);

            var maxFrames = frameSpec.ResolveMaxFrames(spec.MaxFrames);
            var captureAt = frameSpec.ResolveCaptureAtFrame(maxFrames);

            var harnessResult = RaylibOffscreenTestHarness.Run(
                renderer,
                new RaylibOffscreenTestOptions
                {
                    WindowTitle = $"Golden.{spec.StoryId}.{frameSpec.FrameId}",
                    Width = spec.Width,
                    Height = spec.Height,
                    MaxFrames = maxFrames,
                    HideWindow = true,
                    CaptureLastFramePng = captureAt == maxFrames,
                    CaptureAtFrameNumbers = captureAt != maxFrames ? [captureAt] : [],
                });

            if (!harnessResult.RanNativeLoop)
            {
                storySkipped = true;
                storySkipReason = harnessResult.Message ?? "Native offscreen run skipped.";
                frameResults.Add(new GoldenFrameCaptureResult
                {
                    Frame = frameSpec,
                    Skipped = true,
                    SkipReason = storySkipReason,
                });
                continue;
            }

            if (!harnessResult.Succeeded)
            {
                frameResults.Add(new GoldenFrameCaptureResult
                {
                    Frame = frameSpec,
                    Skipped = true,
                    SkipReason = harnessResult.Message ?? "Harness failed.",
                });
                continue;
            }

            var actualPng = harnessResult.FramePngs.TryGetValue(captureAt, out var atPng)
                ? atPng
                : harnessResult.LastFramePng;

            if (actualPng is null)
            {
                frameResults.Add(new GoldenFrameCaptureResult
                {
                    Frame = frameSpec,
                    Skipped = true,
                    SkipReason = $"No PNG captured at frame {captureAt}.",
                });
                continue;
            }

            lastActualPng = actualPng;
            var actualSha = FramebufferAssert.Sha256Hex(actualPng);
            var baselinePath = GoldenCatalog.GetBaselinePngPath(
                assembly,
                storyId,
                spec.IsMultiFrame ? frameSpec.FrameId : null,
                goldensRoot);
            var baselinePng = File.Exists(baselinePath) ? File.ReadAllBytes(baselinePath) : [];
            var baselineSha = baselinePng.Length > 0
                ? FramebufferAssert.Sha256Hex(baselinePng)
                : frameSpec.BaselineSha256;

            bool? frameAssertPassed = null;
            string? frameAssertError = null;

            try
            {
                if (options.Mode == GoldenRunMode.UpdateBaselines)
                {
                    var committedDir = GoldenCatalog.GetStoryDirectory(assembly, storyId, goldensRoot);
                    Directory.CreateDirectory(committedDir);
                    var committedName = GoldenStorySpec.GetCommittedBaselineFileName(spec, frameSpec);
                    File.WriteAllBytes(Path.Combine(committedDir, committedName), actualPng);
                    frameSpec.BaselineSha256 = actualSha;
                    if (!spec.IsMultiFrame)
                        spec.BaselineSha256 = actualSha;
                    frameAssertPassed = true;
                }
                else if (options.Mode == GoldenRunMode.Assert)
                {
                    FramebufferAssert.AssertMatchesBaseline(actualPng, spec, frameSpec, assembly, goldensRoot);
                    frameAssertPassed = true;
                }
            }
            catch (Exception ex)
            {
                frameAssertPassed = false;
                frameAssertError = ex.Message;
            }

            if (options.Mode == GoldenRunMode.UpdateBaselines && baselinePng.Length == 0)
                baselinePng = actualPng;

            frameResults.Add(new GoldenFrameCaptureResult
            {
                Frame = frameSpec,
                ActualPng = actualPng,
                BaselinePng = baselinePng,
                AssertPassed = frameAssertPassed,
                ActualSha256 = actualSha,
                BaselineSha256 = baselineSha,
                ErrorMessage = frameAssertError,
            });
        }

        if (options.Mode == GoldenRunMode.UpdateBaselines)
        {
            var committedDir = GoldenCatalog.GetStoryDirectory(assembly, storyId, goldensRoot);
            spec.SaveToFile(Path.Combine(committedDir, "spec.json"));
        }

        if (storySkipped && frameResults.All(f => f.Skipped))
        {
            var reportPath = GoldenRenderReportWriter.WriteSkippedReport(renderContext, spec, storySkipReason!);
            Console.WriteLine($"Golden render report: {reportPath}");
            return GoldenTestResult.Skip(storySkipReason!, reportPath);
        }

        var anyFail = frameResults.Any(f => f.AssertPassed == false);
        var allPass = frameResults.Count > 0 && frameResults.All(f => f.AssertPassed != false && !f.Skipped);
        bool? storyAssertPassed = options.Mode switch
        {
            GoldenRunMode.Assert => anyFail ? false : allPass ? true : null,
            GoldenRunMode.UpdateBaselines => anyFail ? false : true,
            _ => null,
        };

        var storyAssert = new GoldenStoryAssertInfo
        {
            AssertPassed = storyAssertPassed,
            ErrorMessage = anyFail
                ? string.Join(Environment.NewLine, frameResults
                    .Where(f => !string.IsNullOrWhiteSpace(f.ErrorMessage))
                    .Select(f => $"[{f.Frame.FrameId}] {f.ErrorMessage}"))
                : null,
        };

        var reviewPath = GoldenRenderReportWriter.Write(renderContext, spec, frameResults, storyAssert);
        Console.WriteLine($"Golden render report: {reviewPath}");

        if (anyFail)
            return GoldenTestResult.Fail(storyAssert.ErrorMessage ?? "Golden assert failed", reviewPath, renderContext.StoryDirectory);

        if (lastActualPng is null)
            return GoldenTestResult.Fail("No frames captured.", reviewPath, renderContext.StoryDirectory);

        return GoldenTestResult.Pass(reviewPath, renderContext.StoryDirectory, lastActualPng, storyAssertPassed ?? true);
    }
}
