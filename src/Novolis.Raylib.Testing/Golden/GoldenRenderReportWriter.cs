using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Novolis.Raylib.Testing.Golden;

/// <summary>Writes QA review bundle: index.html, expectations.md, manifest.json, PNGs.</summary>
public static class GoldenRenderReportWriter
{
    private const double RingCircumference = 339.29;

    /// <summary>Writes the full QA review bundle for a golden story run.</summary>
    /// <param name="context">Output paths for this run.</param>
    /// <param name="spec">Story specification.</param>
    /// <param name="frames">Per-frame capture results.</param>
    /// <param name="storyAssert">Aggregate assert outcome.</param>
    /// <returns>Absolute path to <c>index.html</c>.</returns>
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

    /// <summary>Writes a skipped-run QA bundle with no captures.</summary>
    /// <param name="context">Output paths for this run.</param>
    /// <param name="spec">Story specification.</param>
    /// <param name="skipReason">Reason the run was skipped.</param>
    /// <returns>Absolute path to <c>index.html</c>.</returns>
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
        var orderedFrames = OrderFrames(frames).ToList();
        var aggregateStatus = FormatAggregateStatus(storyAssert, frames);
        var runId = Path.GetFileName(context.RunFolder);
        var counts = CountFrameStatuses(orderedFrames);

        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"en\" data-theme=\"dark\"><head><meta charset=\"utf-8\" />");
        sb.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />");
        sb.AppendLine($"<title>{EscapeHtml(spec.Title)} — golden QA</title>");
        AppendStyles(sb);
        sb.AppendLine("</head><body>");
        sb.AppendLine("<div class=\"grain\" aria-hidden=\"true\"></div>");
        sb.AppendLine("<div class=\"shell\">");

        AppendHeader(sb, spec, runId);
        sb.AppendLine("<main>");
        AppendDashboard(sb, aggregateStatus, counts, orderedFrames.Count);
        AppendStoryMeta(sb, spec, aggregateStatus, runId, storyDirectory);

        sb.AppendLine("<div class=\"frames\">");
        var stepNum = 0;
        foreach (var frameResult in orderedFrames)
        {
            stepNum++;
            AppendFrameSection(sb, spec, frameResult, stepNum, storyDirectory);
        }

        sb.AppendLine("</div>");
        sb.AppendLine("</main>");
        AppendFooter(sb);
        AppendThemeScript(sb);
        sb.AppendLine("</div></body></html>");
        return sb.ToString();
    }

    private static void AppendStyles(StringBuilder sb)
    {
        sb.AppendLine("<style>");
        sb.AppendLine(GoldenReportStyles.Css);
        sb.AppendLine("</style>");
    }

    private static void AppendHeader(StringBuilder sb, GoldenStorySpec spec, string runId)
    {
        sb.AppendLine("<header class=\"hdr\">");
        sb.AppendLine("<div class=\"hdr-brand\">");
        sb.AppendLine("<svg class=\"hdr-logo\" viewBox=\"0 0 32 32\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\">");
        sb.AppendLine("<rect width=\"32\" height=\"32\" rx=\"8\" fill=\"url(#golden-lg)\"/>");
        sb.AppendLine("<path d=\"M7 22V10h4.5l3 7.5L18 10h4v12h-3.8v-7.2L15 22h-2.6l-3.2-7.2V22H7z\" fill=\"#fff\"/>");
        sb.AppendLine("<defs><linearGradient id=\"golden-lg\" x1=\"0\" y1=\"0\" x2=\"32\" y2=\"32\"><stop stop-color=\"#6366f1\"/><stop offset=\"1\" stop-color=\"#34d399\"/></linearGradient></defs>");
        sb.AppendLine("</svg>");
        sb.AppendLine("<div class=\"hdr-titles\">");
        sb.Append($"<h1 class=\"hdr-name\">{EscapeHtml(spec.Title)}</h1>");
        sb.AppendLine();
        sb.AppendLine("<span class=\"hdr-sub\">Golden QA Report</span>");
        sb.AppendLine("</div></div>");
        sb.AppendLine("<div class=\"hdr-meta\">");
        sb.Append("<span class=\"chip\">").Append(DateTime.UtcNow.ToString("d MMM yyyy, HH:mm:ss", CultureInfo.InvariantCulture));
        sb.AppendLine(" UTC</span>");
        sb.Append("<span class=\"chip\">Run <code>").Append(EscapeHtml(runId)).AppendLine("</code></span>");
        sb.Append("<span class=\"chip\">Story <code>").Append(EscapeHtml(spec.StoryId)).AppendLine("</code></span>");
        sb.AppendLine("</div>");
        sb.AppendLine("<button type=\"button\" id=\"themeToggle\" class=\"theme-btn\" aria-label=\"Toggle theme\">");
        sb.AppendLine("<svg class=\"theme-icon theme-sun\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\"><circle cx=\"12\" cy=\"12\" r=\"4\"/><path d=\"M12 2v2m0 16v2M4.93 4.93l1.41 1.41m11.32 11.32l1.41 1.41M2 12h2m16 0h2M4.93 19.07l1.41-1.41m11.32-11.32l1.41-1.41\"/></svg>");
        sb.AppendLine("<svg class=\"theme-icon theme-moon\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\"><path d=\"M21 12.79A9 9 0 1111.21 3 7 7 0 0021 12.79z\"/></svg>");
        sb.AppendLine("</button>");
        sb.AppendLine("</header>");
    }

    private static void AppendDashboard(
        StringBuilder sb,
        string aggregateStatus,
        FrameStatusCounts counts,
        int totalFrames)
    {
        var passRate = totalFrames > 0 ? (counts.Passed * 100.0 / totalFrames) : 0;
        var ringColor = aggregateStatus.StartsWith("FAIL", StringComparison.Ordinal)
            ? "var(--rose)"
            : aggregateStatus.StartsWith("SKIPPED", StringComparison.Ordinal)
                ? "var(--amber)"
                : "var(--emerald)";
        var passLen = totalFrames > 0 ? RingCircumference * passRate / 100.0 : 0;
        var gapLen = RingCircumference - passLen;

        sb.AppendLine("<section class=\"dash\" aria-label=\"Story summary\">");
        sb.AppendLine("<div class=\"ring-wrap\">");
        sb.AppendLine("<svg class=\"ring\" viewBox=\"0 0 120 120\" aria-hidden=\"true\">");
        sb.AppendLine("<circle cx=\"60\" cy=\"60\" r=\"54\" fill=\"none\" stroke=\"var(--surface-2)\" stroke-width=\"10\"/>");
        sb.Append("<circle cx=\"60\" cy=\"60\" r=\"54\" fill=\"none\" stroke=\"").Append(ringColor);
        sb.Append("\" stroke-width=\"10\" stroke-linecap=\"round\" stroke-dasharray=\"");
        sb.Append(passLen.ToString("F2", CultureInfo.InvariantCulture));
        sb.Append(' ');
        sb.Append(gapLen.ToString("F2", CultureInfo.InvariantCulture));
        sb.AppendLine("\" transform=\"rotate(-90 60 60)\"/>");
        sb.AppendLine("</svg>");
        sb.Append("<div class=\"ring-center\"><span class=\"ring-pct\">");
        sb.Append(passRate.ToString("F0", CultureInfo.InvariantCulture));
        sb.AppendLine("<small>%</small></span><span class=\"ring-lbl\">pass rate</span></div>");
        sb.AppendLine("</div>");
        sb.AppendLine("<div class=\"stats\">");
        AppendStatCard(sb, "total", counts.Total, "Frames", null);
        AppendStatCard(sb, "passed", counts.Passed, "Passed", "var(--emerald)");
        AppendStatCard(sb, "failed", counts.Failed, "Failed", "var(--rose)");
        AppendStatCard(sb, "skipped", counts.Skipped, "Skipped", "var(--amber)");
        sb.AppendLine("</div>");
        sb.AppendLine("</section>");
    }

    private static void AppendStatCard(StringBuilder sb, string cssClass, int value, string label, string? accent)
    {
        sb.Append("<div class=\"stat ").Append(cssClass).Append('"');
        if (accent is not null)
            sb.Append(" style=\"--accent:").Append(accent).Append('"');
        sb.Append("><span class=\"stat-n\">").Append(value);
        sb.Append("</span><span class=\"stat-l\">").Append(label).AppendLine("</span></div>");
    }

    private static void AppendStoryMeta(
        StringBuilder sb,
        GoldenStorySpec spec,
        string aggregateStatus,
        string runId,
        string storyDirectory)
    {
        sb.Append("<p class=\"story-meta\">");
        sb.Append("Overall: <strong>").Append(EscapeHtml(aggregateStatus)).Append("</strong>");
        sb.Append(" · Story <code>").Append(EscapeHtml(spec.StoryId)).Append("</code>");
        sb.Append(" · Run <code>").Append(EscapeHtml(runId)).Append("</code>");
        sb.Append("<br />Folder: <code>").Append(EscapeHtml(storyDirectory)).AppendLine("</code></p>");
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

        var (statusLabel, badgeClass, grpClass) = FormatFrameStatus(frameResult);

        sb.Append("<div class=\"grp");
        if (!string.IsNullOrEmpty(grpClass))
            sb.Append(' ').Append(grpClass);
        sb.AppendLine(" open\">");
        sb.AppendLine("<div class=\"grp-hd\">");
        sb.AppendLine("<span class=\"grp-indicator\" aria-hidden=\"true\"></span>");
        sb.Append("<div class=\"grp-name\">").Append(stepNum).Append(" — ").Append(EscapeHtml(title));
        sb.Append("<div class=\"grp-caption\">").Append(EscapeHtml(caption)).AppendLine("</div></div>");
        sb.Append("<span class=\"t-badge ").Append(badgeClass).Append("\">").Append(EscapeHtml(statusLabel));
        sb.AppendLine("</span>");
        sb.AppendLine("</div>");
        sb.AppendLine("<div class=\"grp-body-pad\">");

        if (!string.IsNullOrWhiteSpace(frameResult.SkipReason) || !string.IsNullOrWhiteSpace(frameResult.ErrorMessage))
        {
            sb.Append("<p class=\"d-msg\">");
            if (!string.IsNullOrWhiteSpace(frameResult.SkipReason))
                sb.Append(EscapeHtml(frameResult.SkipReason));
            else
                sb.Append(EscapeHtml(frameResult.ErrorMessage!));
            sb.AppendLine("</p>");
        }

        var actualName = GoldenStorySpec.GetAdhocActualFileName(spec, frame);
        var baselineName = GoldenStorySpec.GetAdhocBaselineFileName(spec, frame);
        var actualPath = Path.Combine(storyDirectory, actualName);

        sb.AppendLine("<div class=\"d-sec\">");
        sb.AppendLine("<div class=\"d-lbl\">Actual</div>");
        if (File.Exists(actualPath))
        {
            sb.Append("<img class=\"golden-img\" src=\"").Append(EscapeHtml(actualName)).Append("\" alt=\"");
            sb.Append(EscapeHtml(title)).AppendLine("\" />");
            if (!string.IsNullOrWhiteSpace(frameResult.ActualSha256))
            {
                sb.Append("<div class=\"d-info\">SHA256 <code>").Append(EscapeHtml(frameResult.ActualSha256));
                sb.AppendLine("</code></div>");
            }
        }
        else
            sb.AppendLine("<div class=\"golden-missing\">Frame not captured (native raylib unavailable or harness failed).</div>");

        sb.AppendLine("</div>");

        var baselinePath = Path.Combine(storyDirectory, baselineName);
        if (File.Exists(baselinePath))
        {
            sb.AppendLine("<div class=\"d-sec\">");
            sb.AppendLine("<div class=\"d-lbl\">Baseline</div>");
            sb.Append("<img class=\"golden-img\" src=\"").Append(EscapeHtml(baselineName)).Append("\" alt=\"");
            sb.Append(EscapeHtml(title)).AppendLine(" baseline\" />");
            if (!string.IsNullOrWhiteSpace(frameResult.BaselineSha256))
            {
                sb.Append("<div class=\"d-info\">SHA256 <code>").Append(EscapeHtml(frameResult.BaselineSha256));
                sb.AppendLine("</code></div>");
            }

            sb.AppendLine("</div>");
        }

        if (frame.Expectations.Count > 0)
        {
            sb.AppendLine("<div class=\"d-sec\">");
            sb.AppendLine("<div class=\"d-lbl\">Expected (QA checklist)</div>");
            sb.AppendLine("<ol class=\"d-checklist\">");
            foreach (var item in frame.Expectations)
                sb.Append("<li>").Append(EscapeHtml(item)).AppendLine("</li>");
            sb.AppendLine("</ol></div>");
        }

        sb.AppendLine("</div></div>");
    }

    private static void AppendFooter(StringBuilder sb)
    {
        sb.AppendLine("<footer class=\"report-footer\">");
        sb.AppendLine("<p>Reproduce: call <code>RaylibTestRuntime.EnableForAssembly()</code> in the test assembly, then ");
        sb.AppendLine("<code>dotnet test --filter Category=Golden</code> (or <code>./scripts/run-golden-tests.ps1</code>).</p>");
        sb.AppendLine("</footer>");
    }

    private static void AppendThemeScript(StringBuilder sb)
    {
        sb.AppendLine("""
            <script>
            (function(){
              var root=document.documentElement;
              var btn=document.getElementById('themeToggle');
              if(!btn)return;
              var stored=localStorage.getItem('novolis-golden-report-theme');
              if(stored==='light'||stored==='dark')root.setAttribute('data-theme',stored);
              btn.addEventListener('click',function(){
                var next=root.getAttribute('data-theme')==='light'?'dark':'light';
                root.setAttribute('data-theme',next);
                localStorage.setItem('novolis-golden-report-theme',next);
              });
            })();
            </script>
            """);
    }

    private static IEnumerable<GoldenFrameCaptureResult> OrderFrames(IReadOnlyList<GoldenFrameCaptureResult> frames) =>
        frames.OrderBy(f => f.Frame.FrameId, StringComparer.Ordinal);

    private static (string Label, string BadgeClass, string GrpClass) FormatFrameStatus(GoldenFrameCaptureResult frameResult)
    {
        if (frameResult.Skipped)
            return ("skipped", "skipped", "skip");
        if (frameResult.AssertPassed == false)
            return ("fail", "failed", "fail");
        if (frameResult.AssertPassed == true)
            return ("pass", "passed", "");
        if (frameResult.ActualPng is { Length: > 0 })
            return ("captured", "captured", "");
        return ("not captured", "skipped", "skip");
    }

    private static FrameStatusCounts CountFrameStatuses(IReadOnlyList<GoldenFrameCaptureResult> frames)
    {
        var counts = new FrameStatusCounts { Total = frames.Count };
        foreach (var frame in frames)
        {
            if (frame.Skipped)
                counts.Skipped++;
            else if (frame.AssertPassed == false)
                counts.Failed++;
            else if (frame.AssertPassed == true)
                counts.Passed++;
            else if (frame.ActualPng is { Length: > 0 })
                counts.Passed++;
            else
                counts.Skipped++;
        }

        return counts;
    }

    private struct FrameStatusCounts
    {
        public int Total;
        public int Passed;
        public int Failed;
        public int Skipped;
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
