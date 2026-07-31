using Novolis.CodeGen.Bindings;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

namespace Novolis.Raylib.CodeGen.Unit;

internal static class CodegenTestEnvironment
{
    public static CodegenEnvironment CreateMock(string repoRoot, IReadOnlyDictionary<string, string> relativeFiles)
    {
        // Tests historically used Windows drive roots; MockFileSystem on Linux requires a Unix-rooted path.
        repoRoot = NormalizeMockRoot(repoRoot);

        var files = new Dictionary<string, MockFileData>(StringComparer.OrdinalIgnoreCase);
        foreach (var (relativePath, contents) in relativeFiles)
        {
            var fullPath = Path.Combine(repoRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
            files[fullPath] = new MockFileData(contents);
        }

        var fileSystem = new MockFileSystem(files, repoRoot);
        return new CodegenEnvironment { FileSystem = fileSystem, RepoRoot = repoRoot };
    }

    /// <summary>Cross-platform mock root accepted by <see cref="MockFileSystem"/>.</summary>
    internal static string DefaultMockRoot =>
        OperatingSystem.IsWindows() ? @"C:\novolis\raylib-test" : "/novolis/raylib-test";

    /// <summary>Maps <c>C:\…</c> (and relative) mock roots so IO.Abstractions accepts them on Linux/macOS.</summary>
    internal static string NormalizeMockRoot(string repoRoot)
    {
        if (string.IsNullOrWhiteSpace(repoRoot))
            return DefaultMockRoot;

        // Always rewrite Windows drive-letter paths on non-Windows (Path.IsPathRooted is true for C:\… even on Linux).
        if (!OperatingSystem.IsWindows()
            && repoRoot.Length >= 2
            && char.IsAsciiLetter(repoRoot[0])
            && repoRoot[1] == ':')
        {
            var rest = repoRoot[2..].TrimStart('\\', '/').Replace('\\', '/');
            return string.IsNullOrEmpty(rest) ? DefaultMockRoot : "/" + rest;
        }

        if (Path.IsPathRooted(repoRoot))
            return repoRoot.Replace('\\', Path.DirectorySeparatorChar);

        return Path.GetFullPath(repoRoot);
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
