using Novolis.CodeGen.Bindings;
using Novolis.Raylib.Manifests;

namespace Novolis.Raylib.CodeGen;

public static class FacadeDocEnricher
{
    public static int Enrich(string repoRoot, bool write)
    {
        var env = CodegenEnvironment.Physical(repoRoot);
        _ = RaylibHeaderDocs.Load(env, RaylibHeaderDocs.RaylibHeaderPath(repoRoot));
        _ = RaylibHeaderDocs.Load(env, RaylibHeaderDocs.RayguiHeaderPath(repoRoot));
        var manifests = RaylibBindingManifestSource.Instance;
        var updated = 0;

        foreach (var fragmentId in new[] { "facades", "hud", "gui" })
        {
            var fragment = manifests.GetRequired<FacadeTypesFragment>(FragmentKind.FacadeTypes, fragmentId);
            foreach (var type in fragment.Types)
            {
                foreach (var method in type.Methods)
                {
                    if (!string.IsNullOrWhiteSpace(method.Summary))
                        continue;
                    updated++;
                }
            }
        }

        if (write && updated > 0)
        {
            Console.Error.WriteLine("Facade doc enrichment writes are disabled: maintain summaries in Novolis.Raylib.Manifests C# sources.");
            return 1;
        }

        Console.WriteLine($"enrich-docs: {updated} methods would need summaries (C# manifest authority).");
        return 0;
    }
}
