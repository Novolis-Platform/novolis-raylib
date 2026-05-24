using Novolis.CodeGen.Bindings;
using Novolis.Raylib.CodeGen;
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

        var debugManifest = LoadDebugManifest(context);
        var rewriter = new EndDrawingRewriter(debugManifest);
        return (CompilationUnitSyntax)rewriter.Visit(unit);
    }

    private static DebugManifestDocument LoadDebugManifest(RaylibCodegenContext context)
    {
        var debug = context.DebugConfig;
        if (debug is null)
            return new DebugManifestDocument("EndDrawing", "EndDrawing");

        return new DebugManifestDocument(debug.NotifyAfterNativeCall, debug.FrameHubNotifyAfter);
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
                                SyntaxFactory.ParseName("Novolis.Raylib.Runtime.Presentation.RaylibPresentationHooks"),
                                SyntaxFactory.IdentifierName("Notify")))));
            }

            var attributes = node.AttributeLists
                .Where(list => !list.Attributes.Any(a =>
                    a.Name.ToString().Contains("MethodImpl", StringComparison.Ordinal)));

            return node
                .WithAttributeLists(SyntaxFactory.List(attributes))
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
