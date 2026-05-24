namespace Novolis.Raylib.CodeGen;

public static class FacadeDocVerifier
{
    public static int Verify(string repoRoot)
    {
        var raylibComments = RaylibHeaderDocs.LoadFromFile(RaylibHeaderDocs.RaylibHeaderPath(repoRoot));
        var rayguiComments = RaylibHeaderDocs.LoadFromFile(RaylibHeaderDocs.RayguiHeaderPath(repoRoot));
        var pipelineDir = RepoPaths.PipelineDir(repoRoot);
        var manifestFiles = new[] { "facades.manifest.json", "hud.manifest.json", "gui.manifest.json", "raygui.manifest.json" };
        var errors = new List<string>();

        foreach (var file in manifestFiles)
        {
            var path = Path.Combine(pipelineDir, file);
            if (!File.Exists(path))
                continue;

            var types = FacadeManifestModels.LoadTypes(path);
            foreach (var type in types)
            {
                if (string.IsNullOrWhiteSpace(FacadeDocResolver.ResolveTypeSummary(type)))
                    errors.Add($"{file}: {type.Name} missing typeSummary");

                foreach (var method in type.Methods ?? [])
                {
                    var summary = FacadeDocResolver.ResolveMethodSummary(type, method, raylibComments, rayguiComments);
                    if (string.IsNullOrWhiteSpace(summary))
                        errors.Add($"{file}: {type.Name}.{method.Name} missing summary");
                }
            }
        }

        if (errors.Count == 0)
        {
            Console.WriteLine("verify-docs: OK");
            return 0;
        }

        Console.Error.WriteLine($"verify-docs: {errors.Count} issue(s):");
        foreach (var error in errors)
            Console.Error.WriteLine($"  {error}");
        return 1;
    }
}
