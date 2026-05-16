using Novolis.Raylib.CodeGen;

namespace Novolis.Raylib.CodeGen.Unit;

public sealed class FacadeEmitterTests
{
    [Test]
    public async Task Facade_emitter_writes_type_and_method_summaries_before_method_impl()
    {
        var type = new FacadeTypeDefinition
        {
            Name = "Graphics",
            Namespace = "Novolis.Raylib.Rendering",
            Folder = "Rendering",
            TypeSummary = "2D drawing.",
            Methods =
            [
                new FacadeMethodDefinition
                {
                    Name = "BeginDrawing",
                    Signature = "void BeginDrawing()",
                    Body = "Raylib6Native.BeginDrawing()",
                    Summary = "Setup canvas (framebuffer) to start drawing",
                },
            ],
        };

        var emitted = FacadeEmitter.EmitType(
            type,
            manifestSha256: "test",
            raylibComments: new Dictionary<string, string>(),
            rayguiComments: new Dictionary<string, string>(),
            facadeMethodImpl: "AggressiveInlining");

        await Assert.That(emitted).Contains("/// 2D drawing.");
        await Assert.That(emitted).Contains("/// Setup canvas (framebuffer) to start drawing");
        var summaryIndex = emitted.IndexOf("/// Setup canvas", StringComparison.Ordinal);
        var methodImplIndex = emitted.IndexOf("[MethodImpl(MethodImplOptions.AggressiveInlining)]", StringComparison.Ordinal);
        var methodIndex = emitted.IndexOf("public static void BeginDrawing()", StringComparison.Ordinal);
        await Assert.That(summaryIndex).IsLessThan(methodImplIndex);
        await Assert.That(methodImplIndex).IsLessThan(methodIndex);
    }
}
