# Codegen (not published)

Build-time Roslyn tooling for `Novolis.Raylib.Bindings`. These projects are **never** packed as NuGet packages.

**CI does not run the maintainer pipeline** (`step_01_source`, native fetch, `ci-codegen`, etc.). Run `dotnet run --project codegen/Novolis.Raylib.Pipeline -- run maintainer` locally, commit generated sources under `src/`, then push. GitHub Actions builds and tests those committed results only.

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
