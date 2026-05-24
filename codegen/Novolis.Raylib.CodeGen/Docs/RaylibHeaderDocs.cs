using System.Text.RegularExpressions;

namespace Novolis.Raylib.CodeGen;

internal static partial class RaylibHeaderDocs
{
    private static readonly Regex RlApiLine = new(
        @"^\s*(?:RLAPI|RAYGUIAPI)\s+.+?\s+(\w+)\s*\([^;]*\)\s*;\s*(?://\s*(.+))?\s*$",
        RegexOptions.Compiled | RegexOptions.Multiline);

    public static IReadOnlyDictionary<string, string> LoadFromFile(string headerPath)
    {
        if (!File.Exists(headerPath))
            return new Dictionary<string, string>(StringComparer.Ordinal);

        var text = File.ReadAllText(headerPath);
        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (Match match in RlApiLine.Matches(text))
        {
            var name = match.Groups[1].Value;
            var comment = match.Groups[2].Success ? match.Groups[2].Value.Trim() : "";
            if (!string.IsNullOrWhiteSpace(comment))
                map[name] = comment;
        }

        return map;
    }

    public static string RaylibHeaderPath(string repoRoot) =>
        PipelinePaths.RaylibHeaderPath(repoRoot);

    public static string RayguiHeaderPath(string repoRoot) =>
        PipelinePaths.RayguiHeaderPath(repoRoot);
}
