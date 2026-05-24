# step_03_verify_manifest

Verifies each `raylib-exports.manifest.json` import exists in the fetched `raylib.h`.

## Depends on

- step_01_source

## Refresh

```bash
dotnet run --project codegen/Novolis.Raylib.Pipeline -- run step_03_verify_manifest --force
```
