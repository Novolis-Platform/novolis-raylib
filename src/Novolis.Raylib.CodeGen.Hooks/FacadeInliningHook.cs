using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Novolis.Raylib.CodeGen;

namespace Novolis.Raylib.CodeGen.Hooks;

/// <summary>Adds <c>[MethodImpl(MethodImplOptions.AggressiveInlining)]</c> to expression-bodied façade forwards.</summary>
public sealed class FacadeInliningHook : IRaylibCodegenHook
{
    public int Order => 30;

    public RaylibCodegenPhase Phase => RaylibCodegenPhase.Facade;

    public CompilationUnitSyntax Transform(CompilationUnitSyntax unit, RaylibCodegenContext context)
    {
        if (!string.Equals(context.FacadeMethodImpl, "AggressiveInlining", StringComparison.Ordinal))
            return unit;

        var rewriter = new InliningRewriter();
        var rewritten = (CompilationUnitSyntax)rewriter.Visit(unit)!;
        return rewriter.AddedInlining ? EnsureCompilerServicesUsing(rewritten) : rewritten;
    }

    private static CompilationUnitSyntax EnsureCompilerServicesUsing(CompilationUnitSyntax unit)
    {
        const string ns = "System.Runtime.CompilerServices";
        if (unit.Usings.Any(u => u.Name?.ToString() == ns))
            return unit;

        var usingDirective = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(ns));
        return unit.WithUsings(unit.Usings.Add(usingDirective));
    }

    private sealed class InliningRewriter : CSharpSyntaxRewriter
    {
        public bool AddedInlining { get; private set; }

        public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            var visited = (MethodDeclarationSyntax?)base.VisitMethodDeclaration(node);
            if (visited is null)
                return null;

            if (!visited.Modifiers.Any(SyntaxKind.PublicKeyword) ||
                !visited.Modifiers.Any(SyntaxKind.StaticKeyword))
                return visited;

            if (visited.Body is not null)
                return visited;

            if (visited.ExpressionBody is null)
                return visited;

            if (visited.AttributeLists.Any(list =>
                    list.Attributes.Any(a => a.Name.ToString().Contains("MethodImpl", StringComparison.Ordinal))))
                return visited;

            var attr = SyntaxFactory.AttributeList(
                SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Attribute(
                        SyntaxFactory.ParseName("MethodImpl"),
                        SyntaxFactory.AttributeArgumentList(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.AttributeArgument(
                                    SyntaxFactory.ParseExpression("MethodImplOptions.AggressiveInlining")))))));

            AddedInlining = true;
            return visited.WithAttributeLists(visited.AttributeLists.Add(attr));
        }
    }
}
