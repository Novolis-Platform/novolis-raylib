using Novolis.CodeGen.Bindings;

namespace Novolis.Raylib.CodeGen;

public sealed class RaylibCodegenContext : BindingEmitContext
{
    public required RaylibCodegenPhase Phase { get; init; }

    public IReadOnlyDictionary<string, string> ImportDescriptions { get; init; } =
        new Dictionary<string, string>(StringComparer.Ordinal);

    public string? FacadeTypeName { get; init; }

    public string? FacadeMethodImpl { get; init; }
}
