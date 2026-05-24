# step_01_source

Downloads pinned raylib prebuilts, raygui header, and raylib-cimgui (git clone) into `artifacts/`.

## Inputs

- [`../versions.json`](../versions.json)

## Outputs (under `artifacts/`)

- `raylib-6/include/raylib.h`
- `raylib-6/prebuilt/win-x64/raylib.dll` (Windows)
- `raygui-6/raygui.h`
- `raylib-cimgui/` (with bundled cimgui)

## Refresh

```bash
dotnet run --project codegen/Novolis.Raylib.Pipeline -- run step_01_source --force
```
