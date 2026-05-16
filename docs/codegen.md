# Codegen

Bindings are generated from JSON manifests under `pipeline/raylib6/`.

- `codegen/Novolis.Raylib.CodeGen` — Roslyn CLI (`generate`, `verify`, `suggest-raylib`, `hooks list`; not published)
- **Interop** → `src/Novolis.Raylib.Bindings/Interop/*.g.cs`
- **Façades, Hud, Gui** → `src/Novolis.Raylib.Runtime/**/*.g.cs`
- CI runs `scripts/raylib-codegen-check.ps1` to fail on drift (Bindings + Runtime)

Manifests:

| File | Output |
|------|--------|
| `raylib-exports.manifest.json` | `Raylib6Native.g.cs` |
| `raygui-exports.manifest.json` | `RayguiShimExports.g.cs` |
| `raylib-debug.manifest.json` | `RaylibDebugFrameHooks.g.cs` |
| `facades.manifest.json` | `Graphics`, `World`, `Window`, … |
| `hud.manifest.json` | `Hud` (screen-space 2D) |
| `gui.manifest.json` | `Gui` (raygui widgets) |

Regenerate locally:

```bash
dotnet run --project codegen/Novolis.Raylib.CodeGen -- generate
```
