using System.Reflection;
using Novolis.CodeGen.Bindings;
using Novolis.CodeGen.Bindings.Roslyn;
using Novolis.Raylib.Manifests;

namespace Novolis.Raylib.CodeGen;

internal sealed class RaylibFacadeDocumentationResolver(
    IReadOnlyDictionary<string, string> raylibComments,
    IReadOnlyDictionary<string, string> rayguiComments) : IFacadeDocumentationResolver
{
    public string? ResolveTypeSummary(FacadeTypeSpec type) =>
        FacadeDocResolver.ResolveTypeSummary(type);

    public string? ResolveMethodSummary(FacadeTypeSpec type, FacadeMethodSpec method) =>
        FacadeDocResolver.ResolveMethodSummary(
            type,
            method,
            raylibComments,
            rayguiComments);
}
