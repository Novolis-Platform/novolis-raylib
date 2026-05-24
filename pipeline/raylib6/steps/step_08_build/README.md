# step_08_build

Builds `Novolis.Raylib.Bindings` and `Novolis.Raylib.Runtime` in Release (agent gate).

## Depends on

- step_07_drift

## Refresh

```bash
dotnet run --project codegen/Novolis.Raylib.Pipeline -- run agent-verify --force
```
