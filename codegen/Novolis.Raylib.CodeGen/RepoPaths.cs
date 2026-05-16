namespace Novolis.Raylib.CodeGen;

internal static class RepoPaths
{
    public static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "Directory.Packages.props")))
                return dir.FullName;
            dir = dir.Parent;
        }

        return Directory.GetCurrentDirectory();
    }

    public static string PipelineDir(string repoRoot) =>
        Path.Combine(repoRoot, "pipeline", "raylib6");

    public static string BindingsDir(string repoRoot) =>
        Path.Combine(repoRoot, "src", "Novolis.Raylib.Bindings");

    public static string RuntimeDir(string repoRoot) =>
        Path.Combine(repoRoot, "src", "Novolis.Raylib.Runtime");

    public static string InteropDir(string repoRoot) =>
        Path.Combine(BindingsDir(repoRoot), "Interop");
}
