# Codegen (not published)

Build-time Roslyn tooling for `Novolis.Raylib.Bindings`. These projects are **never** packed as NuGet packages.

| Project | Role |
|---------|------|
| `Novolis.Raylib.CodeGen` | CLI: `generate`, `verify`, `suggest-raylib`, `hooks list` |
| `Novolis.Raylib.CodeGen.Abstractions` | `IRaylibCodegenHook`, `RaylibCodegenContext` |
| `Novolis.Raylib.CodeGen.Hooks` | Shipped Roslyn transforms (inlining, XML docs, debug notify) |

```bash
dotnet run --project codegen/Novolis.Raylib.CodeGen -- generate
```

Published packages live under `src/` only.
