using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Novolis.Raylib.CodeGen;

public interface IRaylibCodegenHook
{
    int Order { get; }

    RaylibCodegenPhase Phase { get; }

    CompilationUnitSyntax Transform(CompilationUnitSyntax unit, RaylibCodegenContext context);
}
