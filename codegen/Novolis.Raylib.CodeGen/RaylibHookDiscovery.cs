using System.Reflection;
using Novolis.CodeGen.Bindings;
using Novolis.CodeGen.Bindings.Roslyn;
using Novolis.Raylib.Manifests;

namespace Novolis.Raylib.CodeGen;

internal static class RaylibHookDiscovery
{
    public static IReadOnlyList<IRaylibCodegenHook> DiscoverAll()
    {
        var assemblies = new List<Assembly> { typeof(RaylibHookDiscovery).Assembly };
        var hooksName = "Novolis.Raylib.CodeGen.Hooks";
        var loaded = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(assembly => string.Equals(assembly.GetName().Name, hooksName, StringComparison.Ordinal));
        if (loaded is not null)
            assemblies.Add(loaded);
        else
        {
            var baseDir = AppContext.BaseDirectory;
            foreach (var path in new[]
                     {
                         Path.Combine(baseDir, $"{hooksName}.dll"),
                         Path.Combine(baseDir, "..", "Novolis.Raylib.CodeGen.Hooks", "bin", "Debug", "net10.0", $"{hooksName}.dll"),
                         Path.Combine(baseDir, "..", "Novolis.Raylib.CodeGen.Hooks", "bin", "Release", "net10.0", $"{hooksName}.dll"),
                     })
            {
                var full = Path.GetFullPath(path);
                if (!File.Exists(full))
                    continue;
                assemblies.Add(Assembly.LoadFrom(full));
                break;
            }
        }

        return HookDiscovery.Discover<RaylibCodegenPhase, RaylibCodegenContext>(assemblies.ToArray())
            .OfType<IRaylibCodegenHook>()
            .ToList();
    }
}
