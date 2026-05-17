using System.Net;
using System.Text;

namespace Novolis.Raylib.Testing.Golden;

/// <summary>
/// Mirrors a Novolis golden QA bundle (<c>index.html</c>, <c>*.actual.png</c>, sidecars)
/// into a stable, browsable folder (e.g. <c>{frameId}.png</c> beside HTML).
/// </summary>
public static class GoldenArtifactPublisher
{
    private static readonly string[] SidecarFileNames =
    [
        "expectations.md",
        "manifest.json",
        "agent-brief.json",
        "story.json",
        "assert.txt",
    ];

    public static GoldenPublishResult Publish(
        string sourceStoryDirectory,
        string destinationDirectory,
        GoldenPublishOptions? options = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceStoryDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationDirectory);
        options ??= new GoldenPublishOptions();

        Directory.CreateDirectory(destinationDirectory);
        var indexPath = Path.Combine(destinationDirectory, "index.html");
        var copiedPngs = new List<string>();

        if (Directory.Exists(sourceStoryDirectory))
        {
            foreach (var file in Directory.EnumerateFiles(sourceStoryDirectory))
            {
                var name = Path.GetFileName(file);
                if (name.Equals("index.html", StringComparison.OrdinalIgnoreCase))
                {
                    File.Copy(file, indexPath, overwrite: true);
                    continue;
                }

                if (options.StablePngNames
                    && name.EndsWith(".actual.png", StringComparison.OrdinalIgnoreCase))
                {
                    var frameId = name[..^".actual.png".Length];
                    var destPng = Path.Combine(destinationDirectory, frameId + ".png");
                    File.Copy(file, destPng, overwrite: true);
                    copiedPngs.Add(destPng);
                    continue;
                }

                if (options.CopySidecarFiles
                    && SidecarFileNames.Contains(name, StringComparer.OrdinalIgnoreCase))
                {
                    File.Copy(file, Path.Combine(destinationDirectory, name), overwrite: true);
                }
            }
        }

        if (!File.Exists(indexPath))
            WriteFallbackIndex(indexPath, destinationDirectory, options);

        if (options.WriteReadme)
            WriteReadme(destinationDirectory, indexPath, options);

        return new GoldenPublishResult
        {
            DestinationDirectory = Path.GetFullPath(destinationDirectory),
            IndexHtmlPath = Path.GetFullPath(indexPath),
            CopiedPngPaths = copiedPngs,
        };
    }

    private static void WriteFallbackIndex(string indexPath, string destinationDirectory, GoldenPublishOptions options)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"en\"><head><meta charset=\"utf-8\" />");
        sb.AppendLine($"<title>{H(options.FallbackTitle)}</title>");
        sb.AppendLine("<style>");
        sb.AppendLine(GoldenReportStyles.Css);
        sb.AppendLine("</style></head><body>");
        sb.AppendLine("<div class=\"grain\" aria-hidden=\"true\"></div>");
        sb.AppendLine("<div class=\"shell\">");
        sb.AppendLine("<header class=\"hdr\"><div class=\"hdr-brand\">");
        sb.AppendLine($"<h1 class=\"hdr-name\">{H(options.FallbackTitle)}</h1>");
        sb.AppendLine("<span class=\"hdr-sub\">Golden QA (fallback)</span></div></header>");
        sb.Append("<p class=\"story-meta\">Folder: <code>").Append(H(destinationDirectory)).AppendLine("</code></p>");
        sb.Append("<p class=\"story-meta\">Status: ");
        if (!string.IsNullOrWhiteSpace(options.SkippedMessage))
            sb.Append("<span class=\"t-badge skipped\">").Append(H(options.SkippedMessage)).Append("</span>");
        else if (!string.IsNullOrWhiteSpace(options.FailedMessage))
            sb.Append("<span class=\"t-badge failed\">").Append(H(options.FailedMessage)).Append("</span>");
        else if (options.ReportOnly)
            sb.Append("<span class=\"t-badge captured\">report-only (see Novolis QA bundle when native capture succeeds)</span>");
        else
            sb.Append("no index.html in source bundle");
        sb.AppendLine("</p></div></body></html>");
        File.WriteAllText(indexPath, sb.ToString(), Encoding.UTF8);
    }

    private static void WriteReadme(string destinationDirectory, string indexPath, GoldenPublishOptions options)
    {
        var sb = new StringBuilder()
            .AppendLine("Raylib golden story — open index.html in a browser.")
            .AppendLine()
            .AppendLine($"index.html: {indexPath}")
            .AppendLine($"file:// URI: {new Uri(indexPath).AbsoluteUri}");

        if (!string.IsNullOrWhiteSpace(options.ReadmeStepSummary))
        {
            sb.AppendLine();
            sb.Append("Steps: ").AppendLine(options.ReadmeStepSummary);
        }

        File.WriteAllText(Path.Combine(destinationDirectory, "README.txt"), sb.ToString(), Encoding.UTF8);
    }

    private static string H(string text) => WebUtility.HtmlEncode(text);
}
