# step_02_native

Builds Novolis native shims (trace, ImGui, raygui) via CMake and copies outputs into `artifacts/`.

## Depends on

- step_01_source

## Outputs (under `artifacts/`)

- Platform shims: `novolis_raylib_trace.*`, `novolis_imgui.*`, `novolis_raygui.*`

## Refresh

```bash
dotnet run --project codegen/Novolis.Raylib.Pipeline -- run step_02_native --force
```
