using Novolis.CodeGen.Bindings;
using System.IO.Abstractions.TestingHelpers;

namespace Novolis.Raylib.Pipeline.Unit;

internal static class PipelineTestEnvironment
{
    public static CodegenEnvironment CreateMock(string repoRoot, IReadOnlyDictionary<string, string> relativeFiles)
    {
        var files = new Dictionary<string, MockFileData>(StringComparer.OrdinalIgnoreCase);
        foreach (var (relativePath, contents) in relativeFiles)
        {
            var fullPath = Path.Combine(repoRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
            files[fullPath] = new MockFileData(contents);
        }

        return new CodegenEnvironment
        {
            FileSystem = new MockFileSystem(files, repoRoot),
            RepoRoot = repoRoot,
        };
    }

    public static string RaylibHeaderRelativePath =>
        Path.Combine(
            "codegen",
            "pipeline",
            "raylib6",
            "steps",
            "step_01_source",
            "artifacts",
            "raylib-6",
            "include",
            "raylib.h");

    public static IBindingManifestSource Manifests(params IManifestFragment[] fragments) =>
        BindingManifestSource.Create(fragments);

    public static InteropExportsFragment Interop(params InteropImportSpec[] imports) =>
        new(
            "raylib6",
            1,
            null,
            null,
            "raylib.dll",
            new InteropPolicySpec([], [], null, false),
            [],
            imports);
}
