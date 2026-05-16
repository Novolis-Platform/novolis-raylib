using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace Novolis.Raylib.CodeGen;

internal static class CodegenFormatter
{
    public static string FormatCompilationUnit(CompilationUnitSyntax unit)
    {
        var workspace = new AdhocWorkspace();
        var formatted = Formatter.Format(unit, workspace);
        return formatted.NormalizeWhitespace(eol: Environment.NewLine).ToFullString();
    }

    public static CompilationUnitSyntax ParseGenerated(string source) =>
        (CompilationUnitSyntax)CSharpSyntaxTree.ParseText(source, path: "").GetRoot();
}
