using Novolis.CodeGen.Bindings;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

namespace Novolis.Raylib.CodeGen.Unit;

internal static class CodegenTestEnvironment
{
    public static CodegenEnvironment CreateMock(string repoRoot, IReadOnlyDictionary<string, string> relativeFiles)
    {
        var files = new Dictionary<string, MockFileData>(StringComparer.OrdinalIgnoreCase);
        foreach (var (relativePath, contents) in relativeFiles)
        {
            var fullPath = Path.Combine(repoRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
            files[fullPath] = new MockFileData(contents);
        }

        var fileSystem = new MockFileSystem(files, repoRoot);
        return new CodegenEnvironment { FileSystem = fileSystem, RepoRoot = repoRoot };
    }

    public static InteropExportsFragment InteropFragment(params InteropImportSpec[] imports) =>
        new(
            Id: "raylib6",
            SchemaVersion: 1,
            Header: null,
            Description: null,
            DllName: "raylib.dll",
            Policy: new InteropPolicySpec([], [], null, false),
            Structs: [],
            Imports: imports);

    public static IBindingManifestSource Manifests(params IManifestFragment[] fragments) =>
        BindingManifestSource.Create(fragments);

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
}
