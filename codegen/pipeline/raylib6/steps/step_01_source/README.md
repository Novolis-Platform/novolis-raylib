# step_01_source

Downloads and verifies pinned raylib 6.2 prebuilts, raygui 5.0, cimgui 1.92.9dock,
the matching ImGui submodule, and ray-cimgui into `artifacts/`.

## Inputs

- [`../versions.json`](../versions.json)

## Outputs (under `artifacts/`)

- `raylib/include/raylib.h`
- `raylib/prebuilt/win-x64/raylib.dll` (Windows)
- `raygui/raygui.h`
- `raylib-cimgui/` (with bundled cimgui and ImGui sources)

## Refresh

```bash
dotnet run --project codegen/Novolis.Raylib.Pipeline -- run step_01_source --force
```
