# step_07_drift

Regenerates docs/codegen and fails if committed manifests or generated C# drift from git.

## Depends on

- step_06_codegen

## Refresh

```bash
dotnet run --project codegen/Novolis.Raylib.Pipeline -- run ci-codegen --force
```
