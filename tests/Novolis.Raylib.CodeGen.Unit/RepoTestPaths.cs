namespace Novolis.Raylib.CodeGen.Unit;

internal static class RepoTestPaths
{
    internal static string? TryRepositoryRoot()
    {
        string? dir;
        try
        {
            var loc = typeof(RepoTestPaths).Assembly.Location;
            dir = string.IsNullOrEmpty(loc) ? null : Path.GetDirectoryName(loc);
        }
        catch
        {
            dir = null;
        }

        if (string.IsNullOrEmpty(dir))
            dir = AppContext.BaseDirectory;

        var d = new DirectoryInfo(Path.GetFullPath(dir));
        for (var i = 0; i < 24 && d is not null; i++, d = d.Parent!)
        {
            if (File.Exists(Path.Combine(d.FullName, "Directory.Packages.props")))
                return d.FullName;
        }

        return null;
    }
}
