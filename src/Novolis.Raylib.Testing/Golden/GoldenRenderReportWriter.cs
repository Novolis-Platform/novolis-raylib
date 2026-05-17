using System.Net;
using System.Text;
using System.Text.Json;

namespace Novolis.Raylib.Testing.Golden;

/// <summary>Writes QA review bundle: index.html, expectations.md, manifest.json, PNGs.</summary>
public static class GoldenRenderReportWriter
{
    public static string Write(
        GoldenRenderRunContext context,
        GoldenStorySpec spec,
        IReadOnlyList<GoldenFrameCaptureResult> frames,
        GoldenStoryAssertInfo storyAssert)
    {
        Directory.CreateDirectory(context.StoryDirectory);

        foreach (var frameResult in frames)
        {
            var actualName = GoldenStorySpec.GetAdhocActualFileName(spec, frameResult.Frame);
            var baselineName = GoldenStorySpec.GetAdhocBaselineFileName(spec, frameResult.Frame);
            if (frameResult.ActualPng is { Length: > 0 })
                File.WriteAllBytes(Path.Combine(context.StoryDirectory, actualName), frameResult.ActualPng);
            if (frameResult.BaselinePng is { Length: > 0 })
                File.WriteAllBytes(Path.Combine(context.StoryDirectory, baselineName), frameResult.BaselinePng);
        }

        WriteExpectationsMarkdown(context.StoryDirectory, spec, frames);
        var htmlPath = WriteIndexHtml(context, spec, frames, storyAssert);
        WriteManifest(context, spec, frames, storyAssert);
        WriteAgentBrief(context, spec, frames, storyAssert);

        var errors = CollectAssertErrors(frames, storyAssert);
        if (!string.IsNullOrWhiteSpace(errors))
            File.WriteAllText(Path.Combine(context.StoryDirectory, "assert.txt"), errors);

        return htmlPath;
    }

    public static string WriteSkippedReport(
        GoldenRenderRunContext context,
        GoldenStorySpec spec,
        string skipReason)
    {
        var frames = spec.GetEffectiveFrames()
            .Select(f => new GoldenFrameCaptureResult
            {
                Frame = f,
                Skipped = true,
                SkipReason = skipReason,
            })
            .ToList();

        return Write(
            context,
            spec,
            frames,
            new GoldenStoryAssertInfo { Skipped = true, SkipReason = skipReason, ErrorMessage = skipReason });
    }

    internal static string BuildIndexHtml(
        GoldenRenderRunContext context,
        GoldenStorySpec spec,
        IReadOnlyList<GoldenFrameCaptureResult> frames,
        GoldenStoryAssertInfo storyAssert,
        string storyDirectory)
    {
        var aggregateStatus = FormatAggregateStatus(storyAssert, frames);
        var runId = Path.GetFileName(context.RunFolder);

        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"en\"><head><meta charset=\"utf-8\" />");
        sb.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />");
        sb.AppendLine($"<title>{EscapeHtml(spec.Title)} — golden QA</title>");
        AppendStyles(sb);
        sb.AppendLine("</head><body>");
        sb.AppendLine($"<h1>{EscapeHtml(spec.Title)}</h1>");
        sb.Append("<p class=\"meta\">Story: <code>").Append(EscapeHtml(spec.StoryId));
        sb.Append("</code> · Status: <strong>").Append(EscapeHtml(aggregateStatus)).AppendLine("</strong>");
        sb.Append(" · Run: ").Append(EscapeHtml(runId));
        sb.Append(" · UTC: ").Append(DateTime.UtcNow.ToString("O"));
        sb.Append("<br />Folder: <code>").Append(EscapeHtml(storyDirectory)).AppendLine("</code></p>");

        var stepNum = 0;
        foreach (var frameResult in OrderFrames(frames))
        {
            stepNum++;
            AppendFrameSection(sb, spec, frameResult, stepNum, storyDirectory);
        }

        sb.AppendLine("<footer><p>Reproduce: call <code>RaylibTestRuntime.EnableForAssembly()</code> in the test assembly, then ");
        sb.AppendLine("<code>dotnet test --filter Category=Golden</code> (or <code>./scripts/run-golden-tests.ps1</code>).</p></footer>");
        sb.AppendLine("</body></html>");
        return sb.ToString();
    }

    private static void AppendStyles(StringBuilder sb)
    {
        sb.AppendLine("<style>");
        sb.AppendLine("body{font-family:Segoe UI,system-ui,sans-serif;background:#0d1117;color:#c9d6e8;margin:0;padding:24px;line-height:1.45;}");
        sb.AppendLine("h1{color:#5dd8ff;font-size:1.5rem;margin:0 0 8px;}");
        sb.AppendLine(".meta{color:#8b949e;font-size:0.9rem;margin-bottom:24px;}");
        sb.AppendLine("section{background:#161b22;border:1px solid #30363d;border-radius:8px;padding:16px 20px;margin-bottom:20px;}");
        sb.AppendLine("section h2{margin:0 0 6px;font-size:1.15rem;color:#c9a227;}");
        sb.AppendLine("section h3{margin:12px 0 6px;font-size:1rem;color:#8b949e;}");
        sb.AppendLine("section p{margin:0 0 12px;color:#8b949e;font-size:0.9rem;}");
        sb.AppendLine("section .status{font-size:0.85rem;margin-bottom:12px;}");
        sb.AppendLine(".ok{color:#79e0a8;}.skip{color:#ff6b5a;}.fail{color:#ff8a7a;}");
        sb.AppendLine("img{max-width:960px;width:100%;height:auto;border:1px solid #21262d;border-radius:4px;display:block;margin-bottom:12px;}");
        sb.AppendLine(".missing{padding:48px;text-align:center;color:#6e7681;background:#0d1117;border:1px dashed #30363d;border-radius:4px;}");
        sb.AppendLine("ol{margin:0;padding-left:1.25rem;color:#c9d6e8;}");
        sb.AppendLine("footer{margin-top:32px;font-size:0.85rem;color:#6e7681;}");
        sb.AppendLine("</style>");
    }

    private static void AppendFrameSection(
        StringBuilder sb,
        GoldenStorySpec spec,
        GoldenFrameCaptureResult frameResult,
        int stepNum,
        string storyDirectory)
    {
        var frame = frameResult.Frame;
        var title = string.IsNullOrWhiteSpace(frame.Title) ? frame.FrameId : frame.Title;
        var caption = string.IsNullOrWhiteSpace(frame.Caption)
            ? "Captured frame from this run."
            : frame.Caption;

        sb.AppendLine("<section>");
        sb.Append("<h2>").Append(stepNum).Append(" — ").Append(EscapeHtml(title)).AppendLine("</h2>");
        sb.Append("<p>").Append(EscapeHtml(caption)).AppendLine("</p>");

        var (statusLabel, statusClass) = FormatFrameStatus(frameResult);
        sb.Append("<p class=\"status ").Append(statusClass).Append("\">Status: ").Append(EscapeHtml(statusLabel));
        if (!string.IsNullOrWhiteSpace(frameResult.SkipReason))
            sb.Append(" — ").Append(EscapeHtml(frameResult.SkipReason));
        else if (!string.IsNullOrWhiteSpace(frameResult.ErrorMessage))
            sb.Append(" — ").Append(EscapeHtml(frameResult.ErrorMessage));
        sb.AppendLine("</p>");

        var actualName = GoldenStorySpec.GetAdhocActualFileName(spec, frame);
        var baselineName = GoldenStorySpec.GetAdhocBaselineFileName(spec, frame);
        var actualPath = Path.Combine(storyDirectory, actualName);

        if (File.Exists(actualPath))
        {
            sb.Append("<img src=\"").Append(EscapeHtml(actualName)).Append("\" alt=\"").Append(EscapeHtml(title)).AppendLine("\" />");
            if (!string.IsNullOrWhiteSpace(frameResult.ActualSha256))
                sb.Append("<p class=\"meta\">Actual SHA256: <code>").Append(EscapeHtml(frameResult.ActualSha256)).AppendLine("</code></p>");
        }
        else
            sb.AppendLine("<div class=\"missing\">Frame not captured (native raylib unavailable or harness failed).</div>");

        var baselinePath = Path.Combine(storyDirectory, baselineName);
        if (File.Exists(baselinePath))
        {
            sb.AppendLine("<h3>Baseline</h3>");
            sb.Append("<img src=\"").Append(EscapeHtml(baselineName)).Append("\" alt=\"").Append(EscapeHtml(title)).AppendLine(" baseline\" />");
            if (!string.IsNullOrWhiteSpace(frameResult.BaselineSha256))
                sb.Append("<p class=\"meta\">Baseline SHA256: <code>").Append(EscapeHtml(frameResult.BaselineSha256)).AppendLine("</code></p>");
        }

        if (frame.Expectations.Count > 0)
        {
            sb.AppendLine("<h3>Expected (QA checklist)</h3>");
            sb.AppendLine("<ol>");
            foreach (var item in frame.Expectations)
                sb.Append("<li>").Append(EscapeHtml(item)).AppendLine("</li>");
            sb.AppendLine("</ol>");
        }

        sb.AppendLine("</section>");
    }

    private static IEnumerable<GoldenFrameCaptureResult> OrderFrames(IReadOnlyList<GoldenFrameCaptureResult> frames) =>
        frames.OrderBy(f => f.Frame.FrameId, StringComparer.Ordinal);

    private static (string Label, string CssClass) FormatFrameStatus(GoldenFrameCaptureResult frameResult)
    {
        if (frameResult.Skipped)
            return ("skipped", "skip");
        if (frameResult.AssertPassed == false)
            return ("fail", "fail");
        if (frameResult.AssertPassed == true)
            return ("pass", "ok");
        if (frameResult.ActualPng is { Length: > 0 })
            return ("captured", "ok");
        return ("not captured", "skip");
    }

    private static string FormatAggregateStatus(GoldenStoryAssertInfo storyAssert, IReadOnlyList<GoldenFrameCaptureResult> frames)
    {
        if (storyAssert.Skipped)
            return $"SKIPPED: {storyAssert.SkipReason ?? "unknown"}";
        if (storyAssert.AssertPassed == false)
            return "FAIL";
        if (storyAssert.AssertPassed == true)
            return "PASS";
        if (frames.Any(f => f.AssertPassed == false))
            return "FAIL";
        if (frames.All(f => f.Skipped))
            return "SKIPPED";
        return "REPORT ONLY";
    }

    private static string WriteIndexHtml(
        GoldenRenderRunContext context,
        GoldenStorySpec spec,
        IReadOnlyList<GoldenFrameCaptureResult> frames,
        GoldenStoryAssertInfo storyAssert)
    {
        var html = BuildIndexHtml(context, spec, frames, storyAssert, context.StoryDirectory);
        var htmlPath = Path.Combine(context.StoryDirectory, "index.html");
        File.WriteAllText(htmlPath, html);
        return htmlPath;
    }

    private static void WriteExpectationsMarkdown(
        string storyDirectory,
        GoldenStorySpec spec,
        IReadOnlyList<GoldenFrameCaptureResult> frames)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# {spec.Title}");
        sb.AppendLine();
        sb.AppendLine($"Story: `{spec.StoryId}`");
        sb.AppendLine();

        foreach (var frameResult in OrderFrames(frames))
        {
            var frame = frameResult.Frame;
            if (spec.IsMultiFrame)
            {
                sb.AppendLine($"## {frame.FrameId}");
                sb.AppendLine();
                if (!string.IsNullOrWhiteSpace(frame.Caption))
                {
                    sb.AppendLine(frame.Caption);
                    sb.AppendLine();
                }
            }
            else
                sb.AppendLine("## Expected (QA checklist)");

            sb.AppendLine();
            var i = 1;
            foreach (var item in frame.Expectations)
            {
                sb.AppendLine($"{i}. {item}");
                i++;
            }

            sb.AppendLine();
        }

        File.WriteAllText(Path.Combine(storyDirectory, "expectations.md"), sb.ToString());
    }

    private static void WriteManifest(
        GoldenRenderRunContext context,
        GoldenStorySpec spec,
        IReadOnlyList<GoldenFrameCaptureResult> frames,
        GoldenStoryAssertInfo storyAssert)
    {
        var frameEntries = frames.Select(f => new
        {
            frameId = f.Frame.FrameId,
            title = f.Frame.Title,
            assertPassed = f.AssertPassed,
            skipped = f.Skipped,
            skipReason = f.SkipReason,
            actualSha256 = f.ActualSha256,
            baselineSha256 = f.BaselineSha256,
            expectations = f.Frame.Expectations,
            files = new
            {
                actualPng = GoldenStorySpec.GetAdhocActualFileName(spec, f.Frame),
                baselinePng = GoldenStorySpec.GetAdhocBaselineFileName(spec, f.Frame),
            },
        }).ToList();

        var manifest = new
        {
            schemaVersion = 2,
            storyId = spec.StoryId,
            title = spec.Title,
            runFolder = context.RunFolder,
            storyDirectory = context.StoryDirectory,
            assertPassed = storyAssert.AssertPassed,
            skipped = storyAssert.Skipped,
            skipReason = storyAssert.SkipReason,
            frames = frameEntries,
            files = new
            {
                indexHtml = "index.html",
                expectationsMd = "expectations.md",
            },
        };

        var json = JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(Path.Combine(context.StoryDirectory, "manifest.json"), json);
    }

    private static void WriteAgentBrief(
        GoldenRenderRunContext context,
        GoldenStorySpec spec,
        IReadOnlyList<GoldenFrameCaptureResult> frames,
        GoldenStoryAssertInfo storyAssert)
    {
        var storyDirectory = context.StoryDirectory;
        var frameBriefs = frames.Select(f => new
        {
            frameId = f.Frame.FrameId,
            title = f.Frame.Title,
            actualPng = File.Exists(Path.Combine(storyDirectory, GoldenStorySpec.GetAdhocActualFileName(spec, f.Frame)))
                ? GoldenStorySpec.GetAdhocActualFileName(spec, f.Frame)
                : null,
            baselinePng = File.Exists(Path.Combine(storyDirectory, GoldenStorySpec.GetAdhocBaselineFileName(spec, f.Frame)))
                ? GoldenStorySpec.GetAdhocBaselineFileName(spec, f.Frame)
                : null,
            expectations = f.Frame.Expectations,
            assertPassed = f.AssertPassed,
            skipped = f.Skipped,
            actualSha256 = f.ActualSha256,
            baselineSha256 = f.BaselineSha256,
        }).ToList();

        var brief = new
        {
            schemaVersion = 2,
            storyId = spec.StoryId,
            title = spec.Title,
            reviewHtml = File.Exists(Path.Combine(storyDirectory, "index.html")) ? "index.html" : null,
            frames = frameBriefs,
            assertPassed = storyAssert.AssertPassed,
            skipped = storyAssert.Skipped,
            skipReason = storyAssert.SkipReason,
            storyDirectory,
            runFolder = context.RunFolder,
        };

        var json = JsonSerializer.Serialize(brief, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(Path.Combine(storyDirectory, "agent-brief.json"), json);
    }

    private static string? CollectAssertErrors(IReadOnlyList<GoldenFrameCaptureResult> frames, GoldenStoryAssertInfo storyAssert)
    {
        var sb = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(storyAssert.ErrorMessage))
            sb.AppendLine(storyAssert.ErrorMessage);

        foreach (var frame in frames)
        {
            if (!string.IsNullOrWhiteSpace(frame.ErrorMessage))
                sb.AppendLine($"[{frame.Frame.FrameId}] {frame.ErrorMessage}");
        }

        return sb.Length > 0 ? sb.ToString().TrimEnd() : null;
    }

    private static string EscapeHtml(string text) => WebUtility.HtmlEncode(text);
}
