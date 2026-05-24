# Codegen (not published)

Build-time Roslyn tooling for `Novolis.Raylib.Bindings`. These projects are **never** packed as NuGet packages.

| Project | Role |
|---------|------|
| `Novolis.Raylib.Pipeline` | Linear maintainer pipeline (`run maintainer`, step folders + `result.json`) |
| `Novolis.Raylib.CodeGen` | Roslyn emitters: `verify`, `suggest-raylib`, `hooks list` |
| `Novolis.Raylib.CodeGen.Abstractions` | `IRaylibCodegenHook`, `IPipelineStep`, `PipelinePaths` |
| `Novolis.Raylib.CodeGen.Hooks` | Shipped Roslyn transforms (inlining, XML docs, debug notify) |

```bash
dotnet run --project codegen/Novolis.Raylib.Pipeline -- run maintainer
dotnet run --project codegen/Novolis.Raylib.Pipeline -- run generate
```

Published packages live under `src/` only.
