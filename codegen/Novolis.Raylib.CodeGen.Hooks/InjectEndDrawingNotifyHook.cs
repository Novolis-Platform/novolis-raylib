using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Novolis.Raylib.CodeGen.Hooks;

/// <summary>Expands façade <c>EndDrawing</c> (per raylib-debug.manifest.json) to notify debug hooks and the frame capture hub.</summary>
public sealed class InjectEndDrawingNotifyHook : IRaylibCodegenHook
{
    public int Order => 20;

    public RaylibCodegenPhase Phase => RaylibCodegenPhase.Facade;

    public CompilationUnitSyntax Transform(CompilationUnitSyntax unit, RaylibCodegenContext context)
    {
        if (!string.Equals(context.FacadeTypeName, "Graphics", StringComparison.Ordinal))
            return unit;

        var debugManifest = LoadDebugManifest(context.RepoRoot);
        var rewriter = new EndDrawingRewriter(debugManifest);
        return (CompilationUnitSyntax)rewriter.Visit(unit);
    }

    private static DebugManifestDocument LoadDebugManifest(string repoRoot)
    {
        var path = Path.Combine(repoRoot, "pipeline", "raylib6", "raylib-debug.manifest.json");
        if (!File.Exists(path))
            return new DebugManifestDocument("EndDrawing", "EndDrawing");

        var json = File.ReadAllText(path);
        var doc = JsonSerializer.Deserialize<DebugManifestDocument>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return doc ?? new DebugManifestDocument("EndDrawing", "EndDrawing");
    }

    private sealed class EndDrawingRewriter(DebugManifestDocument manifest) : CSharpSyntaxRewriter
    {
        public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            if (!string.Equals(node.Identifier.Text, manifest.NotifyAfterNativeCall, StringComparison.Ordinal))
                return base.VisitMethodDeclaration(node);

            var statements = new List<StatementSyntax>
            {
                SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName("Raylib6Native"),
                            SyntaxFactory.IdentifierName(manifest.NotifyAfterNativeCall)))),
                SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName("RaylibDebugFrameHooks"),
                            SyntaxFactory.IdentifierName($"NotifyAfter{manifest.NotifyAfterNativeCall}")))),
            };

            if (string.Equals(manifest.FrameHubNotifyAfter, manifest.NotifyAfterNativeCall, StringComparison.Ordinal))
            {
                statements.Add(
                    SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.ParseName("Novolis.Raylib.Internal.RaylibFrameCaptureHub"),
                                SyntaxFactory.IdentifierName("Notify")))));
            }

            return node
                .WithExpressionBody(null)
                .WithBody(SyntaxFactory.Block(statements))
                .WithSemicolonToken(default);
        }
    }

    private sealed class DebugManifestDocument
    {
        public DebugManifestDocument()
        {
            NotifyAfterNativeCall = "EndDrawing";
            FrameHubNotifyAfter = "EndDrawing";
        }

        public DebugManifestDocument(string notifyAfterNativeCall, string frameHubNotifyAfter)
        {
            NotifyAfterNativeCall = notifyAfterNativeCall;
            FrameHubNotifyAfter = frameHubNotifyAfter;
        }

        public string NotifyAfterNativeCall { get; set; } = "EndDrawing";

        public string FrameHubNotifyAfter { get; set; } = "EndDrawing";
    }
}
