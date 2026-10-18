# Codegen

Bindings are generated from C# manifest fragments under
`codegen/Novolis.Raylib.Manifests/`. The pinned upstream versions and
download hashes live in `codegen/pipeline/raylib6/versions.json`.

## Linear pipeline

Maintainer flow is a **step pipeline** under `codegen/pipeline/raylib6/steps/`. Each step writes:

- `result.json` — status, input hashes, output fingerprints (committed)
- `step.log` — full console capture (committed)
- `artifacts/` — downloads and native outputs (gitignored)

| Step | Role |
|------|------|
| `step_01_source` | Fetch verified raylib 6.2, raygui 5.0, cimgui, ImGui, and ray-cimgui sources into `artifacts/` |
| `step_02_native` | CMake build shims; copy native outputs into `artifacts/` and checked-in RID runtimes |
| `step_02a_android` | Build and stage Android arm64 `libraylib.a` plus the NativeActivity host |
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

Manifest fragments:

| File | Output |
|------|--------|
| `Raylib6InteropManifest.cs` | `Raylib6Native.g.cs` |
| `ImguiShimManifest.cs` | `ImguiShimExports.g.cs` (Bindings) |
| `RayguiShimManifest.cs` | `RayguiShimExports.g.cs` (Raygui add-on) |
| `RaylibDebugManifest.cs` | `RaylibDebugFrameHooks.g.cs` |
| `FacadesManifest.cs` | `Graphics`, `World`, `Window`, … |
| `HudManifest.cs` | `Hud` (screen-space 2D) |
| `GuiManifest.cs` | `Gui` (Dear ImGui) |
| `RayguiFacadeManifest.cs` | `RayGui` (optional raygui add-on) |

Regenerate bindings only:

```bash
dotnet run --project codegen/Novolis.Raylib.Pipeline -- run generate
```

Agent gate (drift + build): `dotnet run --project codegen/Novolis.Raylib.Pipeline -- run agent-verify` — see [agentic-tools/README.md](../agentic-tools/README.md).
