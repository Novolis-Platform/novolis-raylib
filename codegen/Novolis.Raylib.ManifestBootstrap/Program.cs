using Novolis.Raylib.CodeGen.Tools;

var repoRoot = args.Length > 0 ? args[0] : FindRepoRoot();
var outputDir = args.Length > 1
    ? args[1]
    : Path.Combine(repoRoot, "codegen", "Novolis.Raylib.Manifests");

return ManifestCsExporter.Export(repoRoot, outputDir);

static string FindRepoRoot()
{
    var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
    while (dir is not null)
    {
        if (File.Exists(Path.Combine(dir.FullName, "Novolis.Raylib.slnx")))
            return dir.FullName;
        dir = dir.Parent;
    }

    throw new InvalidOperationException("Could not locate novolis-raylib repository root.");
}
