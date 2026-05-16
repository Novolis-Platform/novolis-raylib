using System.Text.Json;
using System.Text.Json.Serialization;

namespace Novolis.Raylib.CodeGen;

internal static class RaylibManifestModels
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public static RaylibManifest Load(string manifestPath)
    {
        var json = File.ReadAllText(manifestPath);
        return JsonSerializer.Deserialize<RaylibManifest>(json, JsonOptions)
               ?? throw new InvalidOperationException($"Failed to parse {manifestPath}");
    }

    public static IReadOnlyList<RaylibImport> LoadImports(string manifestPath) =>
        Load(manifestPath).Imports ?? throw new InvalidOperationException("Manifest has no imports.");

    public static IReadOnlyDictionary<string, string> LoadImportDescriptions(string manifestPath)
    {
        var imports = LoadImports(manifestPath);
        return imports
            .Where(i => !string.IsNullOrWhiteSpace(i.Name) && !string.IsNullOrWhiteSpace(i.Description))
            .ToDictionary(i => i.Name!, i => i.Description!, StringComparer.Ordinal);
    }

    public static RaylibInteropPolicy LoadInteropPolicy(string manifestPath)
    {
        var manifest = Load(manifestPath);
        var policy = manifest.InteropPolicy;
        if (policy is null)
            return RaylibInteropPolicy.Default;

        return new RaylibInteropPolicy
        {
            SuppressGcTransitionByTemplate = new HashSet<string>(
                policy.SuppressGcTransitionByTemplate ?? [],
                StringComparer.Ordinal),
            NeverSuppressGcTransition = new HashSet<string>(
                policy.NeverSuppressGcTransition ?? [],
                StringComparer.Ordinal),
            FacadeMethodImpl = policy.FacadeMethodImpl,
            UseDisableRuntimeMarshalling = policy.UseDisableRuntimeMarshalling ?? false,
        };
    }
}

internal sealed class RaylibManifest
{
    [JsonPropertyName("dllName")]
    public string? DllName { get; set; }

    [JsonPropertyName("structs")]
    public List<RaylibStruct>? Structs { get; set; }

    [JsonPropertyName("imports")]
    public List<RaylibImport>? Imports { get; set; }

    [JsonPropertyName("interopPolicy")]
    public RaylibInteropPolicyJson? InteropPolicy { get; set; }
}

internal sealed class RaylibInteropPolicyJson
{
    [JsonPropertyName("suppressGcTransitionByTemplate")]
    public List<string>? SuppressGcTransitionByTemplate { get; set; }

    [JsonPropertyName("neverSuppressGcTransition")]
    public List<string>? NeverSuppressGcTransition { get; set; }

    [JsonPropertyName("facadeMethodImpl")]
    public string? FacadeMethodImpl { get; set; }

    [JsonPropertyName("useDisableRuntimeMarshalling")]
    public bool? UseDisableRuntimeMarshalling { get; set; }
}

internal sealed class RaylibStruct
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("fields")]
    public List<RaylibField>? Fields { get; set; }
}

internal sealed class RaylibField
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("clrType")]
    public string? ClrType { get; set; }
}

internal sealed class RaylibImport
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("template")]
    public string? Template { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("suppressGcTransition")]
    public bool? SuppressGcTransition { get; set; }
}
