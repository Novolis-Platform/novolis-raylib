using Novolis.CodeGen.Bindings;
using Novolis.CodeGen.Bindings.Roslyn;

namespace Novolis.Raylib.CodeGen;

public interface IRaylibCodegenHook : ICodegenHook<RaylibCodegenPhase, RaylibCodegenContext>;
