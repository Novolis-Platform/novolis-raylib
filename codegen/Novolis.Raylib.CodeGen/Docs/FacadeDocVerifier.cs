using Novolis.CodeGen.Bindings;
using Novolis.Raylib.Manifests;

namespace Novolis.Raylib.CodeGen;

public static class FacadeDocVerifier
{
    public static int Verify(string repoRoot)
    {
        var env = CodegenEnvironment.Physical(repoRoot);
        var raylibComments = RaylibHeaderDocs.Load(env, RaylibHeaderDocs.RaylibHeaderPath(repoRoot));
        var rayguiComments = RaylibHeaderDocs.Load(env, RaylibHeaderDocs.RayguiHeaderPath(repoRoot));
        var manifests = RaylibBindingManifestSource.Instance;
        var errors = new List<string>();

        foreach (var fragmentId in new[] { "facades", "hud", "gui", "raygui" })
        {
            var fragment = manifests.TryGet<FacadeTypesFragment>(FragmentKind.FacadeTypes, fragmentId);
            if (fragment is null)
                continue;

            foreach (var typeSpec in fragment.Types)
            {
                var type = RaylibManifestMapping.ToFacadeType(typeSpec);
                if (string.IsNullOrWhiteSpace(FacadeDocResolver.ResolveTypeSummary(type)))
                    errors.Add($"{fragmentId}: {type.Name} missing typeSummary");

                foreach (var method in type.Methods ?? [])
                {
                    var summary = FacadeDocResolver.ResolveMethodSummary(type, method, raylibComments, rayguiComments);
                    if (string.IsNullOrWhiteSpace(summary))
                        errors.Add($"{fragmentId}: {type.Name}.{method.Name} missing summary");
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
