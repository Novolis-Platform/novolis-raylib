# Codegen workflow (agents)

## When you must run codegen

- Any edit under `codegen/pipeline/raylib6/*.manifest.json`
- Any edit under `codegen/Novolis.Raylib.CodeGen.Hooks/`
- Any task that mentions new raylib exports, façades, `Hud`, `Gui`, or interop

## Standard loop

```bash
# 1. Edit manifests (and hooks if customizing emission)
# 2. Regenerate
dotnet run --project codegen/Novolis.Raylib.Pipeline -- run generate

# 3. Agent gate (drift + build)
dotnet run --project codegen/Novolis.Raylib.Pipeline -- run agent-verify
```

Commit **manifest and generated `*.g.cs` in the same commit**. After maintainer runs, commit updated `codegen/pipeline/raylib6/steps/*/result.json` and `step.log` for steps that ran.

## Where output goes

| Manifest | Generated into |
|----------|----------------|
| `raylib-exports.manifest.json` | `src/Novolis.Raylib.Bindings/Interop/Raylib6Native.g.cs` |
| `imgui-exports.manifest.json` | `Bindings/Interop/ImguiShimExports.g.cs` |
| `raygui-exports.manifest.json` | `Raygui/Interop/RayguiShimExports.g.cs` (add-on) |
| `raylib-debug.manifest.json` | `Bindings/Interop/RaylibDebugFrameHooks.g.cs` |
| `facades.manifest.json` | `Runtime/Rendering`, `Windowing`, `Interact`, … |
| `hud.manifest.json` | `Runtime/Hud/Hud.g.cs` |
| `gui.manifest.json` | `Runtime/Gui/Gui.g.cs` (Dear ImGui) |
| `raygui.manifest.json` | `Raygui/RayGui/RayGui.g.cs` (add-on) |

Vendor sources live under `codegen/pipeline/raylib6/steps/step_01_source/artifacts/` (legacy mirror under `codegen/vendor/`).

## Adding a raylib function

1. Add import row to `raylib-exports.manifest.json` (`name` + `template`).
2. If public API needed, add method to `facades.manifest.json` (or `hud` / `gui`).
3. If template missing, extend `RaylibInteropEmitter` in `codegen/Novolis.Raylib.CodeGen/Emit/`.
4. Run `generate` + `agent.verify`.

## What not to do

- Patch `Raylib6Native.g.cs` or `Graphics.g.cs` directly.
- Put façade `*.g.cs` back under `Bindings/` (they belong in `Runtime/`).
- Add `Novolis.Raylib.CodeGen` to `src/` or pack it as NuGet.

## CI parity

Job `codegen-drift` runs `step_01_source` then `ci-codegen` (`dotnet run --project codegen/Novolis.Raylib.Pipeline -- run ci-codegen`).

## Debugging failures

1. Scan `codegen/pipeline/raylib6/steps/*/result.json` for `"status": "Failed"`.
2. Read that step’s `step.log`.
3. Re-run: `dotnet run --project codegen/Novolis.Raylib.Pipeline -- run step_XX --force`
