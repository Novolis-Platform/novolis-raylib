using System.Reflection;
using Novolis.CodeGen.Bindings;
using Novolis.CodeGen.Bindings.Roslyn;
using Novolis.Raylib.Manifests;

namespace Novolis.Raylib.CodeGen;

internal sealed class RaylibDebugHooksEmitterAdapter : IBindingEmitter
{
    public EmitStrategy Strategy => EmitStrategy.DebugHooks;

    public string Emit(EmitRequest request) =>
        RaylibDebugHooksEmitter.Emit((DebugConfigFragment)request.Fragment, request.ManifestSha256);
}
