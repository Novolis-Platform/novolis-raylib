using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Novolis.Raylib.CodeGen.Hooks;

/// <summary>Expands <c>Graphics.EndDrawing</c> to notify debug hooks after the native call.</summary>
public sealed class InjectEndDrawingNotifyHook : IRaylibCodegenHook
{
    public int Order => 20;

    public RaylibCodegenPhase Phase => RaylibCodegenPhase.Facade;

    public CompilationUnitSyntax Transform(CompilationUnitSyntax unit, RaylibCodegenContext context)
    {
        if (!string.Equals(context.FacadeTypeName, "Graphics", StringComparison.Ordinal))
            return unit;

        var rewriter = new EndDrawingRewriter();
        return (CompilationUnitSyntax)rewriter.Visit(unit);
    }

    private sealed class EndDrawingRewriter : CSharpSyntaxRewriter
    {
        public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            if (node.Identifier.Text != "EndDrawing")
                return base.VisitMethodDeclaration(node);

            var statements = SyntaxFactory.Block(
                SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName("Raylib6Native"),
                            SyntaxFactory.IdentifierName("EndDrawing")))),
                SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName("RaylibDebugFrameHooks"),
                            SyntaxFactory.IdentifierName("NotifyAfterEndDrawing")))),
                SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.ParseName("Novolis.Raylib.Internal.RaylibFrameCaptureHub"),
                            SyntaxFactory.IdentifierName("Notify")))));

            return node
                .WithExpressionBody(null)
                .WithBody(statements)
                .WithSemicolonToken(default);
        }
    }
}
