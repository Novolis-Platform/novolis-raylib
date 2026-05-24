# Codegen (not published)

Build-time Roslyn tooling for `Novolis.Raylib.Bindings`. These projects are **never** packed as NuGet packages.

**CI does not run the maintainer pipeline** (`step_01_source`, native fetch, `ci-codegen`, etc.). Run `dotnet run --project codegen/Novolis.Raylib.Pipeline -- run maintainer` locally, commit generated sources under `src/`, then push. GitHub Actions builds and tests those committed results only.

| Project | Role |
|---------|------|
| `pipeline/raylib6/` (this folder) | Manifests, step folders (`result.json`, `step.log`, gitignored `artifacts/`) |
| `native/` (this folder) | CMake shims (trace, ImGui, raygui) |
| `vendor/` (this folder) | Legacy vendor mirror (optional; sources live under `pipeline/.../artifacts/`) |
| `Novolis.Raylib.Pipeline` | Linear maintainer pipeline (`run maintainer`, step folders + `result.json`) |
| `Novolis.Raylib.CodeGen` | Roslyn emitters: `verify`, `suggest-raylib`, `hooks list` |
| `Novolis.Raylib.CodeGen.Abstractions` | `IRaylibCodegenHook`, `IPipelineStep`, `PipelinePaths` |
| `Novolis.Raylib.CodeGen.Hooks` | Shipped Roslyn transforms (inlining, XML docs, debug notify) |

```bash
# Regenerate committed *.g.cs under src/ (after manifest or hook edits)
dotnet run --project codegen/Novolis.Raylib.Pipeline -- run generate

# Full maintainer flow (fetch, native shims, codegen, drift)
dotnet run --project codegen/Novolis.Raylib.Pipeline -- run maintainer

# CI/agent gate: drift check + Release build
dotnet run --project codegen/Novolis.Raylib.Pipeline -- run agent-verify

dotnet run --project codegen/Novolis.Raylib.Pipeline -- list
```

There is no `scripts/` folder — use Pipeline profiles and `dotnet test` / `dotnet pack` directly.

Published packages live under `src/` only.
