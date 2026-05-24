using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Novolis.CodeGen.Bindings;

namespace Novolis.Raylib.CodeGen.Tools;

/// <summary>One-time migration: reads legacy JSON manifests and emits C# fragment definitions.</summary>
internal static class ManifestCsExporter
{
    private static readonly JsonSerializerOptions JsonRead = new() { PropertyNameCaseInsensitive = true };

    public static int Export(string repoRoot, string outputProjectDir)
    {
        var pipelineDir = Path.Combine(repoRoot, "codegen", "pipeline", "raylib6");
        Directory.CreateDirectory(outputProjectDir);

        var fragments = new List<(string FileName, string TypeName, string PropertyName, IManifestFragment Fragment)>
        {
            ("Raylib6InteropManifest.cs", "InteropExportsFragment", "Raylib6Interop", LoadInterop(Path.Combine(pipelineDir, "raylib-exports.manifest.json"))),
            ("ImguiShimManifest.cs", "ShimExportsFragment", "ImguiShim", LoadShim(Path.Combine(pipelineDir, "imgui-exports.manifest.json"), "imgui", "novolis_imgui")),
            ("RayguiShimManifest.cs", "ShimExportsFragment", "RayguiShim", LoadShim(Path.Combine(pipelineDir, "raygui-exports.manifest.json"), "raygui", "novolis_raygui")),
            ("RaylibDebugManifest.cs", "DebugConfigFragment", "RaylibDebug", LoadDebug(Path.Combine(pipelineDir, "raylib-debug.manifest.json"))),
            ("FacadesManifest.cs", "FacadeTypesFragment", "Facades", LoadFacade(Path.Combine(pipelineDir, "facades.manifest.json"), "facades")),
            ("HudManifest.cs", "FacadeTypesFragment", "Hud", LoadFacade(Path.Combine(pipelineDir, "hud.manifest.json"), "hud")),
            ("GuiManifest.cs", "FacadeTypesFragment", "Gui", LoadFacade(Path.Combine(pipelineDir, "gui.manifest.json"), "gui")),
            ("RayguiFacadeManifest.cs", "FacadeTypesFragment", "RayguiFacade", LoadFacade(Path.Combine(pipelineDir, "raygui.manifest.json"), "raygui")),
        };

        foreach (var (fileName, _, propertyName, fragment) in fragments)
        {
            var code = EmitFragmentFile(propertyName, fragment);
            File.WriteAllText(Path.Combine(outputProjectDir, fileName), code, Encoding.UTF8);
        }

        File.WriteAllText(
            Path.Combine(outputProjectDir, "RaylibBindingManifestSource.cs"),
            EmitSourceFile(fragments.Select(f => (f.PropertyName, f.TypeName)).ToList()),
            Encoding.UTF8);

        Console.WriteLine($"Exported {fragments.Count} manifest fragments to {outputProjectDir}");
        return 0;
    }

    private static string EmitSourceFile(IReadOnlyList<(string PropertyName, string TypeName)> entries)
    {
        var sb = new StringBuilder();
        sb.AppendLine("using Novolis.CodeGen.Bindings;");
        sb.AppendLine();
        sb.AppendLine("namespace Novolis.Raylib.Manifests;");
        sb.AppendLine();
        sb.AppendLine("public sealed class RaylibBindingManifestSource : IBindingManifestSource");
        sb.AppendLine("{");
        sb.AppendLine("    public static RaylibBindingManifestSource Instance { get; } = new();");
        sb.AppendLine();
        sb.AppendLine("    private RaylibBindingManifestSource() =>");
        sb.AppendLine("        Fragments =");
        sb.AppendLine("        [");
        foreach (var (propertyName, _) in entries)
            sb.AppendLine($"            RaylibBindingManifests.{propertyName},");
        sb.AppendLine("        ];");
        sb.AppendLine();
        sb.AppendLine("    public IReadOnlyList<IManifestFragment> Fragments { get; }");
        sb.AppendLine("}");
        return sb.ToString();
    }

    private static string EmitFragmentFile(string propertyName, IManifestFragment fragment)
    {
        var sb = new StringBuilder();
        sb.AppendLine("using Novolis.CodeGen.Bindings;");
        sb.AppendLine();
        sb.AppendLine("namespace Novolis.Raylib.Manifests;");
        sb.AppendLine();
        sb.AppendLine("public static partial class RaylibBindingManifests");
        sb.AppendLine("{");
        sb.Append("    public static ").Append(GetTypeName(fragment)).Append(' ').Append(propertyName).Append(" { get; } = ");
        EmitFragmentExpression(sb, fragment, indent: 4);
        sb.AppendLine(";");
        sb.AppendLine("}");
        return sb.ToString();
    }

    private static string GetTypeName(IManifestFragment fragment) =>
        fragment switch
        {
            InteropExportsFragment => "InteropExportsFragment",
            ShimExportsFragment => "ShimExportsFragment",
            DebugConfigFragment => "DebugConfigFragment",
            FacadeTypesFragment => "FacadeTypesFragment",
            _ => throw new NotSupportedException(),
        };

    private static void EmitFragmentExpression(StringBuilder sb, IManifestFragment fragment, int indent)
    {
        switch (fragment)
        {
            case InteropExportsFragment interop:
                sb.AppendLine("new(");
                WriteLine(sb, indent + 4, $"Id: {Lit(interop.Id)},");
                WriteLine(sb, indent + 4, $"SchemaVersion: {interop.SchemaVersion},");
                WriteLine(sb, indent + 4, $"Header: {Lit(interop.Header)},");
                WriteLine(sb, indent + 4, $"Description: {Lit(interop.Description)},");
                WriteLine(sb, indent + 4, $"DllName: {Lit(interop.DllName)},");
                sb.Append(' ', indent + 4).AppendLine("Policy: new(");
                EmitStringList(sb, interop.Policy.SuppressGcTransitionByTemplate, indent + 8, "SuppressGcTransitionByTemplate");
                EmitStringList(sb, interop.Policy.NeverSuppressGcTransition, indent + 8, "NeverSuppressGcTransition");
                WriteLine(sb, indent + 8, $"FacadeMethodImpl: {Lit(interop.Policy.FacadeMethodImpl)},");
                WriteLine(sb, indent + 8, $"UseDisableRuntimeMarshalling: {Lit(interop.Policy.UseDisableRuntimeMarshalling)}),");
                EmitStructList(sb, interop.Structs, indent + 4);
                EmitImportList(sb, interop.Imports, indent + 4);
                sb.Append(' ', indent).Append(')');
                break;
            case ShimExportsFragment shim:
                sb.AppendLine("new(");
                WriteLine(sb, indent + 4, $"Id: {Lit(shim.Id)},");
                WriteLine(sb, indent + 4, $"SchemaVersion: {shim.SchemaVersion},");
                WriteLine(sb, indent + 4, $"Header: {Lit(shim.Header)},");
                WriteLine(sb, indent + 4, $"Description: {Lit(shim.Description)},");
                WriteLine(sb, indent + 4, $"ModuleFileName: {Lit(shim.ModuleFileName)},");
                EmitShimExportList(sb, shim.Exports, indent + 4);
                sb.Append(' ', indent).Append(')');
                break;
            case DebugConfigFragment debug:
                sb.AppendLine("new(");
                WriteLine(sb, indent + 4, $"Id: {Lit(debug.Id)},");
                WriteLine(sb, indent + 4, $"SchemaVersion: {debug.SchemaVersion},");
                WriteLine(sb, indent + 4, $"Description: {Lit(debug.Description)},");
                WriteLine(sb, indent + 4, $"NotifyAfterNativeCall: {Lit(debug.NotifyAfterNativeCall)},");
                WriteLine(sb, indent + 4, $"FrameHubNotifyAfter: {Lit(debug.FrameHubNotifyAfter)},");
                WriteLine(sb, indent + 4, $"CaptureEnvVar: {Lit(debug.CaptureEnvVar)},");
                WriteLine(sb, indent + 4, $"CapturePngFileType: {Lit(debug.CapturePngFileType)},");
                sb.Append(' ', indent + 4).AppendLine("Symbols: new(");
                WriteLine(sb, indent + 8, $"LoadImageFromScreen: {Lit(debug.Symbols.LoadImageFromScreen)},");
                WriteLine(sb, indent + 8, $"ExportImageToMemory: {Lit(debug.Symbols.ExportImageToMemory)},");
                WriteLine(sb, indent + 8, $"UnloadImage: {Lit(debug.Symbols.UnloadImage)},");
                WriteLine(sb, indent + 8, $"MemFree: {Lit(debug.Symbols.MemFree)}");
                sb.Append(' ', indent + 4).AppendLine(")");
                sb.Append(' ', indent).Append(')');
                break;
            case FacadeTypesFragment facade:
                sb.AppendLine("new(");
                WriteLine(sb, indent + 4, $"Id: {Lit(facade.Id)},");
                EmitFacadeTypeList(sb, facade.Types, indent + 4);
                sb.Append(' ', indent).Append(')');
                break;
        }
    }

    private static void EmitStructList(StringBuilder sb, IReadOnlyList<InteropStructSpec> structs, int indent)
    {
        sb.Append(' ', indent).AppendLine("Structs: new InteropStructSpec[]");
        sb.Append(' ', indent).AppendLine("{");
        foreach (var st in structs)
        {
            sb.Append(' ', indent + 4).Append("new(").Append(Lit(st.Name)).Append(", new InteropFieldSpec[] { ");
            sb.Append(string.Join(", ", st.Fields.Select(f => $"new({Lit(f.Name)}, {Lit(f.ClrType)})")));
            sb.AppendLine(" }),");
        }

        sb.Append(' ', indent).AppendLine("},");
    }

    private static void EmitImportList(StringBuilder sb, IReadOnlyList<InteropImportSpec> imports, int indent)
    {
        sb.Append(' ', indent).AppendLine("Imports: new InteropImportSpec[]");
        sb.Append(' ', indent).AppendLine("{");
        foreach (var import in imports)
        {
            sb.Append(' ', indent + 4).Append("new(")
                .Append(Lit(import.Name)).Append(", ").Append(Lit(import.Template));
            if (import.Description is not null || import.SuppressGcTransition is not null)
            {
                sb.Append(", ").Append(Lit(import.Description));
                if (import.SuppressGcTransition is { } suppress)
                    sb.Append(", ").Append(suppress ? "true" : "false");
            }

            sb.AppendLine("),");
        }

        sb.Append(' ', indent).AppendLine("}");
    }

    private static void EmitShimExportList(StringBuilder sb, IReadOnlyList<ShimExportSpec> exports, int indent)
    {
        sb.Append(' ', indent).AppendLine("Exports: new ShimExportSpec[]");
        sb.Append(' ', indent).AppendLine("{");
        foreach (var export in exports)
            sb.Append(' ', indent + 4).AppendLine($"new({Lit(export.Export)}, {Lit(export.Template)}),");
        sb.Append(' ', indent).AppendLine("}");
    }

    private static void EmitFacadeTypeList(StringBuilder sb, IReadOnlyList<FacadeTypeSpec> types, int indent)
    {
        sb.Append(' ', indent).AppendLine("Types: new FacadeTypeSpec[]");
        sb.Append(' ', indent).AppendLine("{");
        foreach (var type in types)
        {
            sb.Append(' ', indent + 4).AppendLine("new(");
            WriteLine(sb, indent + 8, $"Name: {Lit(type.Name)},");
            WriteLine(sb, indent + 8, $"Namespace: {Lit(type.Namespace)},");
            WriteLine(sb, indent + 8, $"Folder: {Lit(type.Folder)},");
            WriteLine(sb, indent + 8, $"TypeSummary: {Lit(type.TypeSummary)},");
            EmitStringList(sb, type.Usings, indent + 8, "Usings", trailingComma: true);
            sb.Append(' ', indent + 8).AppendLine("Methods: new FacadeMethodSpec[]");
            sb.Append(' ', indent + 8).AppendLine("{");
            foreach (var method in type.Methods)
            {
                sb.Append(' ', indent + 12).Append("new(")
                    .Append(Lit(method.Name)).Append(", ")
                    .Append(Lit(method.Signature)).Append(", ")
                    .Append(Lit(method.Body));
                if (method.Summary is not null)
                    sb.Append(", ").Append(Lit(method.Summary));
                sb.AppendLine("),");
            }

            sb.Append(' ', indent + 8).AppendLine("}");
            sb.Append(' ', indent + 4).AppendLine("),");
        }

        sb.Append(' ', indent).AppendLine("}");
    }

    private static void EmitStringList(StringBuilder sb, IReadOnlyList<string> values, int indent, string? propertyName = null, bool trailingComma = false)
    {
        if (propertyName is not null)
            sb.Append(' ', indent).Append(propertyName).Append(": ");
        else
            sb.Append(' ', indent);
        sb.AppendLine("new string[]");
        sb.Append(' ', indent).AppendLine("{");
        foreach (var value in values)
            sb.Append(' ', indent + 4).AppendLine($"{Lit(value)},");
        sb.Append(' ', indent).Append("}" + (propertyName is not null || trailingComma ? "," : string.Empty)).AppendLine();
    }

    private static void WriteLine(StringBuilder sb, int indent, string line) =>
        sb.Append(' ', indent).AppendLine(line);

    private static string Lit(string? value) =>
        value is null ? "null" : "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n") + "\"";

    private static string Lit(bool value) => value ? "true" : "false";

    private static InteropExportsFragment LoadInterop(string path)
    {
        var doc = JsonSerializer.Deserialize<InteropWire>(File.ReadAllText(path), JsonRead)
                  ?? throw new InvalidOperationException(path);
        var policy = doc.InteropPolicy ?? new();
        return new InteropExportsFragment(
            "raylib6",
            doc.SchemaVersion ?? 1,
            doc.Header,
            doc.Description,
            doc.DllName ?? "raylib",
            new InteropPolicySpec(
                policy.SuppressGcTransitionByTemplate ?? [],
                policy.NeverSuppressGcTransition ?? [],
                policy.FacadeMethodImpl,
                policy.UseDisableRuntimeMarshalling ?? false),
            (doc.Structs ?? []).Select(s => new InteropStructSpec(
                s.Name ?? "",
                (s.Fields ?? []).Select(f => new InteropFieldSpec(f.Name ?? "", f.ClrType ?? "")).ToList())).ToList(),
            (doc.Imports ?? []).Select(i => new InteropImportSpec(
                i.Name ?? "",
                i.Template ?? "",
                i.Description,
                i.SuppressGcTransition)).ToList());
    }

    private static ShimExportsFragment LoadShim(string path, string id, string module)
    {
        var doc = JsonSerializer.Deserialize<ShimWire>(File.ReadAllText(path), JsonRead)
                  ?? throw new InvalidOperationException(path);
        return new ShimExportsFragment(
            id,
            doc.SchemaVersion ?? 1,
            doc.Header,
            doc.Description,
            module,
            (doc.Functions ?? []).Select(f => new ShimExportSpec(f.Export ?? "", f.Template ?? "")).ToList());
    }

    private static DebugConfigFragment LoadDebug(string path)
    {
        var doc = JsonSerializer.Deserialize<DebugWire>(File.ReadAllText(path), JsonRead)
                  ?? throw new InvalidOperationException(path);
        var symbols = doc.Symbols ?? throw new InvalidOperationException(path);
        return new DebugConfigFragment(
            "raylib-debug",
            doc.SchemaVersion ?? 1,
            doc.Description,
            doc.NotifyAfterNativeCall ?? "EndDrawing",
            doc.FrameHubNotifyAfter ?? "EndDrawing",
            doc.CaptureEnvVar ?? "NOVOLIS_RAYLIB_DEBUG_CAPTURE",
            doc.CapturePngFileType ?? ".png",
            new DebugSymbolMapSpec(
                symbols.LoadImageFromScreen ?? "",
                symbols.ExportImageToMemory ?? "",
                symbols.UnloadImage ?? "",
                symbols.MemFree ?? ""));
    }

    private static FacadeTypesFragment LoadFacade(string path, string id)
    {
        var doc = JsonSerializer.Deserialize<FacadeWire>(File.ReadAllText(path), JsonRead)
                  ?? throw new InvalidOperationException(path);
        return new FacadeTypesFragment(
            id,
            (doc.Types ?? []).Select(t => new FacadeTypeSpec(
                t.Name ?? "",
                t.Namespace ?? "",
                t.Folder ?? "",
                t.TypeSummary,
                t.Usings ?? [],
                (t.Methods ?? []).Select(m => new FacadeMethodSpec(
                    m.Name ?? "",
                    m.Signature ?? "",
                    m.Body ?? "",
                    m.Summary)).ToList())).ToList());
    }

    private sealed class InteropWire
    {
        public int? SchemaVersion { get; set; }
        public string? Header { get; set; }
        public string? Description { get; set; }
        public string? DllName { get; set; }
        public InteropPolicyWire? InteropPolicy { get; set; }
        public List<InteropStructWire>? Structs { get; set; }
        public List<InteropImportWire>? Imports { get; set; }
    }

    private sealed class InteropPolicyWire
    {
        public List<string>? SuppressGcTransitionByTemplate { get; set; }
        public List<string>? NeverSuppressGcTransition { get; set; }
        public string? FacadeMethodImpl { get; set; }
        public bool? UseDisableRuntimeMarshalling { get; set; }
    }

    private sealed class InteropStructWire
    {
        public string? Name { get; set; }
        public List<InteropFieldWire>? Fields { get; set; }
    }

    private sealed class InteropFieldWire
    {
        public string? Name { get; set; }
        public string? ClrType { get; set; }
    }

    private sealed class InteropImportWire
    {
        public string? Name { get; set; }
        public string? Template { get; set; }
        public string? Description { get; set; }
        public bool? SuppressGcTransition { get; set; }
    }

    private sealed class ShimWire
    {
        public int? SchemaVersion { get; set; }
        public string? Header { get; set; }
        public string? Description { get; set; }
        public List<ShimFunctionWire>? Functions { get; set; }
    }

    private sealed class ShimFunctionWire
    {
        public string? Export { get; set; }
        public string? Template { get; set; }
    }

    private sealed class DebugWire
    {
        public int? SchemaVersion { get; set; }
        public string? Description { get; set; }
        public string? NotifyAfterNativeCall { get; set; }
        public string? FrameHubNotifyAfter { get; set; }
        public string? CaptureEnvVar { get; set; }
        public string? CapturePngFileType { get; set; }
        public DebugSymbolsWire? Symbols { get; set; }
    }

    private sealed class DebugSymbolsWire
    {
        public string? LoadImageFromScreen { get; set; }
        public string? ExportImageToMemory { get; set; }
        public string? UnloadImage { get; set; }
        public string? MemFree { get; set; }
    }

    private sealed class FacadeWire
    {
        public List<FacadeTypeWire>? Types { get; set; }
    }

    private sealed class FacadeTypeWire
    {
        public string? Name { get; set; }
        public string? Namespace { get; set; }
        public string? Folder { get; set; }
        public string? TypeSummary { get; set; }
        public List<string>? Usings { get; set; }
        public List<FacadeMethodWire>? Methods { get; set; }
    }

    private sealed class FacadeMethodWire
    {
        public string? Name { get; set; }
        public string? Signature { get; set; }
        public string? Body { get; set; }
        public string? Summary { get; set; }
    }
}
