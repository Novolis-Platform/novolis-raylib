using Novolis.CodeGen.Bindings;

namespace Novolis.Raylib.CodeGen.Unit;

public sealed class FacadeEmitterTests
{
    [Test]
    public async Task Facade_emitter_writes_type_and_method_summaries_before_method_impl()
    {
        var fragment = new FacadeTypesFragment(
            "facades",
            [
                new FacadeTypeSpec(
                    "Graphics",
                    "Novolis.Raylib.Rendering",
                    "Rendering",
                    "2D drawing.",
                    [],
                    [new FacadeMethodSpec(
                        "BeginDrawing",
                        "void BeginDrawing()",
                        "Raylib6Native.BeginDrawing()",
                        "Setup canvas (framebuffer) to start drawing")]),
            ]);
        var context = new BindingEmitContext
        {
            Environment = CodegenEnvironment.Physical(Path.GetTempPath()),
            OutputPath = Path.Combine(Path.GetTempPath(), "Graphics.g.cs"),
            Fragment = fragment,
            ManifestSha256 = "test",
            RegenerateHint = "dotnet run --project codegen/Novolis.Raylib.Pipeline -- run generate",
        };
        var emitted = new FacadeForwardEmitter().Emit(
            new EmitRequest(
                fragment,
                "test",
                new EmitTarget(
                    "Graphics",
                    EmitStrategy.FacadeForward,
                    "Graphics.g.cs",
                    "Novolis.Raylib.Rendering",
                    "Novolis.Raylib.Runtime",
                    FacadeMethodImpl: "AggressiveInlining"),
                context));

        await Assert.That(emitted).Contains("/// 2D drawing.");
        await Assert.That(emitted).Contains("/// Setup canvas (framebuffer) to start drawing");
        var summaryIndex = emitted.IndexOf("/// Setup canvas", StringComparison.Ordinal);
        var methodImplIndex = emitted.IndexOf("[MethodImpl(MethodImplOptions.AggressiveInlining)]", StringComparison.Ordinal);
        var methodIndex = emitted.IndexOf("public static void BeginDrawing()", StringComparison.Ordinal);
        await Assert.That(summaryIndex).IsLessThan(methodImplIndex);
        await Assert.That(methodImplIndex).IsLessThan(methodIndex);
    }
}
