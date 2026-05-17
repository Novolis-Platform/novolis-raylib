# Raylib codegen hooks

Roslyn transforms run after each emitter and before files are written. Add a hook by creating a class in `codegen/Novolis.Raylib.CodeGen.Hooks/` that implements `IRaylibCodegenHook`.

## Skeleton

```csharp
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Novolis.Raylib.CodeGen;

namespace Novolis.Raylib.CodeGen.Hooks;

public sealed class MyHook : IRaylibCodegenHook
{
    public int Order => 100;
    public RaylibCodegenPhase Phase => RaylibCodegenPhase.Interop;

    public CompilationUnitSyntax Transform(CompilationUnitSyntax unit, RaylibCodegenContext context)
    {
        // Return a new CompilationUnitSyntax (use CSharpSyntaxRewriter for tree walks).
        return unit;
    }
}
```

## Commands

```bash
dotnet run --project codegen/Novolis.Raylib.CodeGen -- hooks list
dotnet run --project codegen/Novolis.Raylib.CodeGen -- generate
```

Codegen also runs automatically when building `StarConflictsRevolt.Raylib6.Bindings` (see MSBuild target in that project).

## Shipped hooks

| Hook | Phase | Purpose |
|------|-------|---------|
| `AnnotateLibraryImportHook` | Interop | XML docs from optional `description` on `raylib-exports.manifest.json` rows |
| `InjectEndDrawingNotifyHook` | Facade | After native `EndDrawing` (per `raylib-debug.manifest.json`): `RaylibDebugFrameHooks.NotifyAfterEndDrawing()` and `RaylibFrameCaptureHub.Notify()` |
| `FacadeInliningHook` | Facade | `[MethodImpl(AggressiveInlining)]` on expression-bodied forwards when `interopPolicy.facadeMethodImpl` is set |

## `interopPolicy` (manifest v2)

Top-level block on **`raylib-exports.manifest.json`** drives emitter attributes and façade inlining. See [`.cursor/skills/raylib-raygui-interop/SKILL.md`](../../../.cursor/skills/raylib-raygui-interop/SKILL.md).

**`SuppressGCTransition`:** only for imports where native does not re-enter the CLR or block for a long time. **`neverSuppressGcTransition`** excludes symbols like `ExportImageToMemory` and `MemFree`.

**`DisableRuntimeMarshalling`:** when enabled on the Bindings project, string imports use **`Utf8StringMarshaller`** via parameter `[MarshalUsing]`; `SuppressGCTransition` is not combined with it in generated interop.

Do not edit committed `*.g.cs` files by hand—change manifests and/or hooks, then regenerate.
