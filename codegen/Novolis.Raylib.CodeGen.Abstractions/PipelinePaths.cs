namespace Novolis.Raylib.CodeGen;

public static class PipelinePaths
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

    public static string CodegenRoot(string repoRoot) =>
        Path.Combine(repoRoot, "codegen");

    public static string PipelineRaylibDir(string repoRoot) =>
        Path.Combine(CodegenRoot(repoRoot), "pipeline", "raylib6");

    public static string VendorRoot(string repoRoot) =>
        Path.Combine(CodegenRoot(repoRoot), "vendor");

    public static string NativeRoot(string repoRoot) =>
        Path.Combine(CodegenRoot(repoRoot), "native");

    public static string StepsRoot(string repoRoot) =>
        Path.Combine(PipelineRaylibDir(repoRoot), "steps");

    public static string StepDir(string repoRoot, string stepId) =>
        Path.Combine(StepsRoot(repoRoot), stepId);

    public static string StepArtifactsDir(string repoRoot, string stepId) =>
        Path.Combine(StepDir(repoRoot, stepId), "artifacts");

    public static string VersionsJson(string repoRoot) =>
        Path.Combine(PipelineRaylibDir(repoRoot), "versions.json");

    public static string SourceArtifactsDir(string repoRoot) =>
        StepArtifactsDir(repoRoot, "step_01_source");

    public static string RaylibRoot(string repoRoot) =>
        Path.Combine(SourceArtifactsDir(repoRoot), "raylib-6");

    public static string RaylibIncludeDir(string repoRoot) =>
        Path.Combine(RaylibRoot(repoRoot), "include");

    public static string RaylibHeaderPath(string repoRoot) =>
        Path.Combine(RaylibIncludeDir(repoRoot), "raylib.h");

    public static string RaylibPrebuiltWinDir(string repoRoot) =>
        Path.Combine(RaylibRoot(repoRoot), "prebuilt", "win-x64");

    public static string RaylibPrebuiltLinuxDir(string repoRoot) =>
        Path.Combine(RaylibRoot(repoRoot), "prebuilt", "linux-x64");

    public static string RaylibPrebuiltOsxDir(string repoRoot) =>
        Path.Combine(RaylibRoot(repoRoot), "prebuilt", "osx");

    public static string RayguiRoot(string repoRoot) =>
        Path.Combine(SourceArtifactsDir(repoRoot), "raygui-6");

    public static string RayguiHeaderPath(string repoRoot) =>
        Path.Combine(RayguiRoot(repoRoot), "raygui.h");

    public static string RaylibCimguiRoot(string repoRoot) =>
        Path.Combine(SourceArtifactsDir(repoRoot), "raylib-cimgui");

    public static string NativeArtifactsDir(string repoRoot) =>
        StepArtifactsDir(repoRoot, "step_02_native");

    public static string NativeShimOutDir(string repoRoot, string shimFolder) =>
        Path.Combine(NativeRoot(repoRoot), shimFolder, "out");
}
