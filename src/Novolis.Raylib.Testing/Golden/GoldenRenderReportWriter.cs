using System.Net;
using System.Text;
using System.Text.Json;

namespace Novolis.Raylib.Testing.Golden;

/// <summary>Writes QA review bundle: index.html, expectations.md, manifest.json, PNGs.</summary>
public static class GoldenRenderReportWriter
{
    public sealed class AssertInfo
    {
        public bool? AssertPassed { get; init; }

        public string? ActualSha256 { get; init; }

        public string? BaselineSha256 { get; init; }

        public string? ErrorMessage { get; init; }

        public bool Skipped { get; init; }

        public string? SkipReason { get; init; }
    }

    public static string Write(
        GoldenRenderRunContext context,
        GoldenStorySpec spec,
        ReadOnlySpan<byte> actualPng,
        ReadOnlySpan<byte> baselinePng,
        AssertInfo assert)
    {
        Directory.CreateDirectory(context.StoryDirectory);

        var actualPath = Path.Combine(context.StoryDirectory, "actual.png");
        var baselinePath = Path.Combine(context.StoryDirectory, "baseline.png");
        File.WriteAllBytes(actualPath, actualPng);
        if (baselinePng.Length > 0)
            File.WriteAllBytes(baselinePath, baselinePng);

        WriteExpectationsMarkdown(context.StoryDirectory, spec);
        var htmlPath = WriteIndexHtml(context.StoryDirectory, spec, assert);
        WriteManifest(context, spec, assert);
        if (!string.IsNullOrWhiteSpace(assert.ErrorMessage))
            File.WriteAllText(Path.Combine(context.StoryDirectory, "assert.txt"), assert.ErrorMessage);

        return htmlPath;
    }

    public static string WriteSkippedReport(
        GoldenRenderRunContext context,
        GoldenStorySpec spec,
        string skipReason)
    {
        Directory.CreateDirectory(context.StoryDirectory);
        WriteExpectationsMarkdown(context.StoryDirectory, spec);
        var assert = new AssertInfo { Skipped = true, SkipReason = skipReason };
        var htmlPath = WriteIndexHtml(context.StoryDirectory, spec, assert);
        WriteManifest(context, spec, assert);
        File.WriteAllText(Path.Combine(context.StoryDirectory, "assert.txt"), skipReason);
        return htmlPath;
    }

    private static void WriteExpectationsMarkdown(string storyDirectory, GoldenStorySpec spec)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# {spec.Title}");
        sb.AppendLine();
        sb.AppendLine($"Story: `{spec.StoryId}`");
        sb.AppendLine();
        sb.AppendLine("## Expected (QA checklist)");
        sb.AppendLine();
        var i = 1;
        foreach (var item in spec.Expectations)
        {
            sb.AppendLine($"{i}. {item}");
            i++;
        }

        File.WriteAllText(Path.Combine(storyDirectory, "expectations.md"), sb.ToString());
    }

    private static string WriteIndexHtml(string storyDirectory, GoldenStorySpec spec, AssertInfo assert)
    {
        var status = assert.Skipped
            ? $"SKIPPED: {EscapeHtml(assert.SkipReason ?? "unknown")}"
            : assert.AssertPassed == true
                ? "PASS"
                : assert.AssertPassed == false
                    ? "FAIL"
                    : "REPORT ONLY";

        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"en\"><head><meta charset=\"utf-8\"/>");
        sb.AppendLine($"<title>{EscapeHtml(spec.Title)} — golden QA</title>");
        sb.AppendLine("<style>");
        sb.AppendLine("body{font-family:system-ui,sans-serif;margin:24px;max-width:1200px;}");
        sb.AppendLine("table{border-collapse:collapse;width:100%;}");
        sb.AppendLine("th,td{border:1px solid #ccc;padding:12px;vertical-align:top;}");
        sb.AppendLine("th{background:#f4f4f4;text-align:left;}");
        sb.AppendLine("img{max-width:100%;height:auto;border:1px solid #ddd;}");
        sb.AppendLine("ol{margin:0;padding-left:1.25rem;}");
        sb.AppendLine(".meta{color:#555;font-size:0.9rem;}");
        sb.AppendLine("</style></head><body>");
        sb.AppendLine($"<h1>{EscapeHtml(spec.Title)}</h1>");
        sb.AppendLine($"<p class=\"meta\">Story <code>{EscapeHtml(spec.StoryId)}</code> · Status: <strong>{status}</strong></p>");
        sb.AppendLine("<table><thead><tr><th>Render</th><th>Expected (QA checklist)</th></tr></thead><tbody><tr>");
        sb.AppendLine("<td>");
        if (!assert.Skipped && File.Exists(Path.Combine(storyDirectory, "actual.png")))
        {
            sb.AppendLine("<p><strong>Actual</strong></p><img src=\"actual.png\" alt=\"actual\"/>");
            if (File.Exists(Path.Combine(storyDirectory, "baseline.png")))
                sb.AppendLine("<p><strong>Baseline</strong></p><img src=\"baseline.png\" alt=\"baseline\"/>");
        }
        else
            sb.AppendLine("<p><em>No render captured.</em></p>");

        if (!string.IsNullOrWhiteSpace(assert.ActualSha256))
            sb.AppendLine($"<p class=\"meta\">Actual SHA256: <code>{EscapeHtml(assert.ActualSha256)}</code></p>");
        if (!string.IsNullOrWhiteSpace(assert.BaselineSha256))
            sb.AppendLine($"<p class=\"meta\">Baseline SHA256: <code>{EscapeHtml(assert.BaselineSha256)}</code></p>");
        sb.AppendLine("</td><td><ol>");
        foreach (var item in spec.Expectations)
            sb.AppendLine($"<li>{EscapeHtml(item)}</li>");
        sb.AppendLine("</ol></td></tr></tbody></table>");
        sb.AppendLine("</body></html>");

        var htmlPath = Path.Combine(storyDirectory, "index.html");
        File.WriteAllText(htmlPath, sb.ToString());
        return htmlPath;
    }

    private static void WriteManifest(GoldenRenderRunContext context, GoldenStorySpec spec, AssertInfo assert)
    {
        var manifest = new
        {
            schemaVersion = 1,
            storyId = spec.StoryId,
            title = spec.Title,
            runFolder = context.RunFolder,
            storyDirectory = context.StoryDirectory,
            assertPassed = assert.AssertPassed,
            skipped = assert.Skipped,
            skipReason = assert.SkipReason,
            actualSha256 = assert.ActualSha256,
            baselineSha256 = assert.BaselineSha256,
            expectations = spec.Expectations,
            files = new
            {
                indexHtml = "index.html",
                expectationsMd = "expectations.md",
                actualPng = "actual.png",
                baselinePng = "baseline.png",
            },
        };

        var json = JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(Path.Combine(context.StoryDirectory, "manifest.json"), json);
    }

    private static string EscapeHtml(string text) => WebUtility.HtmlEncode(text);
}
