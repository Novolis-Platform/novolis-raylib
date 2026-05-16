using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Novolis.Raylib.CodeGen.Hooks;

/// <summary>Adds XML documentation to manifest-generated <c>[LibraryImport]</c> methods when the manifest row has a <c>description</c>.</summary>
public sealed class AnnotateLibraryImportHook : IRaylibCodegenHook
{
    public int Order => 10;

    public RaylibCodegenPhase Phase => RaylibCodegenPhase.Interop;

    public CompilationUnitSyntax Transform(CompilationUnitSyntax unit, RaylibCodegenContext context)
    {
        if (context.ImportDescriptions.Count == 0)
            return unit;

        var rewriter = new AnnotateRewriter(context.ImportDescriptions);
        return (CompilationUnitSyntax)rewriter.Visit(unit);
    }

    private sealed class AnnotateRewriter : CSharpSyntaxRewriter
    {
        private readonly IReadOnlyDictionary<string, string> _descriptions;

        public AnnotateRewriter(IReadOnlyDictionary<string, string> descriptions) =>
            _descriptions = descriptions;

        public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            var visited = (MethodDeclarationSyntax?)base.VisitMethodDeclaration(node);
            if (visited is null)
                return null;

            if (visited.Modifiers.Any(SyntaxKind.StaticKeyword) != true)
                return visited;

            if (!_descriptions.TryGetValue(visited.Identifier.Text, out var description))
                return visited;

            if (visited.HasLeadingTrivia && visited.GetLeadingTrivia().Any(t => t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)))
                return visited;

            var line = SyntaxFactory.Trivia(
                SyntaxFactory.DocumentationComment(
                    SyntaxFactory.XmlText($" <summary>{EscapeXml(description)}</summary>")));

            return visited.WithLeadingTrivia(visited.GetLeadingTrivia().Insert(0, line));
        }

        private static string EscapeXml(string text) =>
            text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
    }
}
