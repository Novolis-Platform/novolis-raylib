namespace Novolis.Raylib.CodeGen;

/// <summary>Resolved interop optimization policy from <c>raylib-exports.manifest.json</c>.</summary>
internal sealed class RaylibInteropPolicy
{
    public static readonly RaylibInteropPolicy Default = new();

    public HashSet<string> SuppressGcTransitionByTemplate { get; init; } =
        new(StringComparer.Ordinal);

    public HashSet<string> NeverSuppressGcTransition { get; init; } =
        new(StringComparer.Ordinal);

    public string? FacadeMethodImpl { get; init; }

    public bool UseDisableRuntimeMarshalling { get; init; }

    public bool ShouldSuppressGcTransition(RaylibImport import)
    {
        if (import.SuppressGcTransition.HasValue)
            return import.SuppressGcTransition.Value;

        var name = import.Name ?? "";
        if (NeverSuppressGcTransition.Contains(name))
            return false;

        var template = import.Template ?? "";
        return SuppressGcTransitionByTemplate.Contains(template);
    }
}
