# step_06_codegen

Regenerates committed `*.g.cs` interop and façade files from manifests + Roslyn hooks.

## Depends on

- step_03_verify_manifest

## Outputs

- `src/Novolis.Raylib.Bindings/Interop/*.g.cs`
- `src/Novolis.Raylib.Runtime/**/*.g.cs`
- `src/Novolis.Raylib.Raygui/**/*.g.cs`

## Refresh

```bash
dotnet run --project codegen/Novolis.Raylib.Pipeline -- run step_06_codegen --force
```
