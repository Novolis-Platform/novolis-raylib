using Novolis.Raylib.Manifests;

namespace Novolis.Raylib.Pipeline.Steps;

internal static class CodegenOutputCatalog
{
    public static IReadOnlyList<string> AllGeneratedFiles(string repoRoot)
    {
        var list = new List<string>();
        list.AddRange(Directory.GetFiles(Path.Combine(repoRoot, "src", "Novolis.Raylib.Bindings", "Interop"), "*.g.cs"));
        list.AddRange(Directory.GetFiles(Path.Combine(repoRoot, "src", "Novolis.Raylib.Runtime", "Rendering"), "*.g.cs"));
        list.AddRange(Directory.GetFiles(Path.Combine(repoRoot, "src", "Novolis.Raylib.Runtime", "Windowing"), "*.g.cs"));
        list.AddRange(Directory.GetFiles(Path.Combine(repoRoot, "src", "Novolis.Raylib.Runtime", "Interact"), "*.g.cs"));
        list.AddRange(Directory.GetFiles(Path.Combine(repoRoot, "src", "Novolis.Raylib.Runtime", "Timing"), "*.g.cs"));
        list.AddRange(Directory.GetFiles(Path.Combine(repoRoot, "src", "Novolis.Raylib.Runtime", "Audio"), "*.g.cs"));
        list.AddRange(Directory.GetFiles(Path.Combine(repoRoot, "src", "Novolis.Raylib.Runtime", "Hud"), "*.g.cs"));
        var gui = Path.Combine(repoRoot, "src", "Novolis.Raylib.Runtime", "Gui", "Gui.g.cs");
        if (File.Exists(gui))
            list.Add(gui);
        var rayguiInterop = Path.Combine(repoRoot, "src", "Novolis.Raylib.Raygui", "Interop", "RayguiShimExports.g.cs");
        if (File.Exists(rayguiInterop))
            list.Add(rayguiInterop);
        var raygui = Path.Combine(repoRoot, "src", "Novolis.Raylib.Raygui", "RayGui", "RayGui.g.cs");
        if (File.Exists(raygui))
            list.Add(raygui);
        return list;
    }
}
