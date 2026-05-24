# Codegen

Bindings are generated from JSON manifests under `codegen/pipeline/raylib6/`.

## Linear pipeline

Maintainer flow is a **step pipeline** under `codegen/pipeline/raylib6/steps/`. Each step writes:

- `result.json` — status, input hashes, output fingerprints (committed)
- `step.log` — full console capture (committed)
- `artifacts/` — downloads and native outputs (gitignored)

| Step | Role |
|------|------|
| `step_01_source` | Fetch raylib, raygui, raylib-cimgui into `artifacts/` |
| `step_02_native` | CMake build shims; copy DLLs into `artifacts/` |
| `step_03_verify_manifest` | Manifest imports vs `raylib.h` |
| `step_04_enrich_docs` | Fill façade summaries from headers |
| `step_05_verify_docs` | Fail on missing docs |
| `step_06_codegen` | Emit committed `*.g.cs` |
| `step_07_drift` | `git diff` on manifests + generated C# |
| `step_08_build` | Release build Bindings + Runtime |

```bash
dotnet run --project codegen/Novolis.Raylib.Pipeline -- run maintainer
dotnet run --project codegen/Novolis.Raylib.Pipeline -- run generate
dotnet run --project codegen/Novolis.Raylib.Pipeline -- run ci-codegen
dotnet run --project codegen/Novolis.Raylib.Pipeline -- list
```

On failure: open the first `steps/*/result.json` with `"status": "Failed"`, then read that folder’s `step.log`.

## Projects

- `codegen/Novolis.Raylib.Pipeline` — linear orchestrator (profiles + steps)
- `codegen/Novolis.Raylib.CodeGen` — Roslyn emitters (`verify`, `suggest-raylib`, `hooks list`; not published)
- **Interop** → `src/Novolis.Raylib.Bindings/Interop/*.g.cs`
- **Façades, Hud, Gui** → `src/Novolis.Raylib.Runtime/**/*.g.cs`
- **RayGui add-on** → `src/Novolis.Raylib.Raygui/**/*.g.cs`

Manifests:

| File | Output |
|------|--------|
| `raylib-exports.manifest.json` | `Raylib6Native.g.cs` |
| `imgui-exports.manifest.json` | `ImguiShimExports.g.cs` (Bindings) |
| `raygui-exports.manifest.json` | `RayguiShimExports.g.cs` (Raygui add-on) |
| `raylib-debug.manifest.json` | `RaylibDebugFrameHooks.g.cs` |
| `facades.manifest.json` | `Graphics`, `World`, `Window`, … |
| `hud.manifest.json` | `Hud` (screen-space 2D) |
| `gui.manifest.json` | `Gui` (Dear ImGui) |
| `raygui.manifest.json` | `RayGui` (optional raygui add-on) |

Regenerate bindings only:

```bash
dotnet run --project codegen/Novolis.Raylib.Pipeline -- run generate
```

Agent gate (drift + build): `dotnet run --project codegen/Novolis.Raylib.Pipeline -- run agent-verify` — see [agentic-tools/README.md](../agentic-tools/README.md).
