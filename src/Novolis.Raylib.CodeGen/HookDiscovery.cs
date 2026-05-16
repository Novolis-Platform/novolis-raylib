using System.Reflection;

namespace Novolis.Raylib.CodeGen;

internal static class HookDiscovery
{
    public static IReadOnlyList<IRaylibCodegenHook> DiscoverAll()
    {
        var hooks = new List<IRaylibCodegenHook>();
        foreach (var assembly in EnumerateHookAssemblies())
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsAbstract || type.IsInterface || !typeof(IRaylibCodegenHook).IsAssignableFrom(type))
                    continue;

                if (Activator.CreateInstance(type) is IRaylibCodegenHook hook)
                    hooks.Add(hook);
            }
        }

        return hooks.OrderBy(h => h.Order).ThenBy(h => h.GetType().FullName, StringComparer.Ordinal).ToList();
    }

    private static IEnumerable<Assembly> EnumerateHookAssemblies()
    {
        yield return typeof(HookDiscovery).Assembly;

        var hooksName = "Novolis.Raylib.CodeGen.Hooks";
        var loaded = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => string.Equals(a.GetName().Name, hooksName, StringComparison.Ordinal));
        if (loaded is not null)
        {
            yield return loaded;
            yield break;
        }

        var baseDir = AppContext.BaseDirectory;
        var candidates = new[]
        {
            Path.Combine(baseDir, $"{hooksName}.dll"),
            Path.Combine(baseDir, "..", "Novolis.Raylib.CodeGen.Hooks", "bin", "Debug", "net10.0", $"{hooksName}.dll"),
            Path.Combine(baseDir, "..", "Novolis.Raylib.CodeGen.Hooks", "bin", "Release", "net10.0", $"{hooksName}.dll"),
        };

        foreach (var path in candidates)
        {
            var full = Path.GetFullPath(path);
            if (!File.Exists(full))
                continue;

            yield return Assembly.LoadFrom(full);
            yield break;
        }
    }
}
