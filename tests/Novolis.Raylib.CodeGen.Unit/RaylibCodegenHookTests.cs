using Microsoft.CodeAnalysis.CSharp;
using Novolis.Raylib.CodeGen;
using Novolis.Raylib.CodeGen.Hooks;

namespace Novolis.Raylib.CodeGen.Unit;

public sealed class RaylibCodegenHookTests
{
    [Test]
    public async Task InjectEndDrawingNotifyHook_adds_notify_after_native_call()
    {
        const string source = """
            #nullable enable
            using Novolis.Raylib.Interop;
            namespace Novolis.Raylib.Rendering;
            public static partial class Graphics
            {
                public static void EndDrawing() => Raylib6Native.EndDrawing();
            }
            """;

        var unit = CodegenFormatter.ParseGenerated(source);
        var hook = new InjectEndDrawingNotifyHook();
        var context = new RaylibCodegenContext
        {
            RepoRoot = "",
            Phase = RaylibCodegenPhase.Facade,
            OutputPath = "",
            ManifestPath = "",
            ManifestSha256 = "",
            RegenerateHint = "",
            FacadeTypeName = "Graphics",
        };

        var transformed = hook.Transform(unit, context);
        var text = transformed.ToFullString();
        await Assert.That(text).Contains("Raylib6Native.EndDrawing()");
        await Assert.That(text).Contains("RaylibDebugFrameHooks.NotifyAfterEndDrawing()");
        await Assert.That(text).Contains("RaylibPresentationHooks.Notify()");
    }

    [Test]
    public async Task AnnotateLibraryImportHook_adds_xml_doc_from_manifest_description()
    {
        const string source = """
            #nullable enable
            using System.Runtime.InteropServices;
            namespace Novolis.Raylib.Interop;
            internal static partial class Raylib6Native
            {
                private const string RaylibDll = "raylib";
                [LibraryImport(RaylibDll)]
                internal static partial void SetTargetFPS(int value);
            }
            """;

        var unit = CodegenFormatter.ParseGenerated(source);
        var hook = new AnnotateLibraryImportHook();
        var context = new RaylibCodegenContext
        {
            RepoRoot = "",
            Phase = RaylibCodegenPhase.Interop,
            OutputPath = "",
            ManifestPath = "",
            ManifestSha256 = "",
            RegenerateHint = "",
            ImportDescriptions = new Dictionary<string, string>
            {
                ["SetTargetFPS"] = "Sets the target frame rate for the game loop.",
            },
        };

        var transformed = hook.Transform(unit, context);
        var text = transformed.ToFullString();
        await Assert.That(text).Contains("Sets the target frame rate for the game loop");
    }

    [Test]
    public async Task FacadeInliningHook_adds_aggressive_inlining_to_expression_bodied_methods()
    {
        const string source = """
            #nullable enable
            using Novolis.Raylib.Interop;
            namespace Novolis.Raylib.Rendering;
            public static partial class Graphics
            {
                public static void BeginDrawing() => Raylib6Native.BeginDrawing();
                public static void EndDrawing()
                {
                    Raylib6Native.EndDrawing();
                }
            }
            """;

        var unit = CodegenFormatter.ParseGenerated(source);
        var hook = new FacadeInliningHook();
        var context = new RaylibCodegenContext
        {
            RepoRoot = "",
            Phase = RaylibCodegenPhase.Facade,
            OutputPath = "",
            ManifestPath = "",
            ManifestSha256 = "",
            RegenerateHint = "",
            FacadeTypeName = "Graphics",
            FacadeMethodImpl = "AggressiveInlining",
        };

        var transformed = hook.Transform(unit, context);
        var text = transformed.ToFullString();
        await Assert.That(text).Contains("MethodImpl(MethodImplOptions.AggressiveInlining)");
        await Assert.That(text).Contains("BeginDrawing() => Raylib6Native.BeginDrawing()");
        var endDrawingIndex = text.IndexOf("void EndDrawing()", StringComparison.Ordinal);
        var inliningAfterEndDrawing = text.IndexOf(
            "MethodImpl(MethodImplOptions.AggressiveInlining)",
            endDrawingIndex,
            StringComparison.Ordinal);
        await Assert.That(inliningAfterEndDrawing).IsEqualTo(-1);
    }
}
