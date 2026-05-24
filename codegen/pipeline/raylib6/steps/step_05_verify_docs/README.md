# step_05_verify_docs

Fails if façade/Hud/Gui/raygui manifest types or methods lack resolvable summaries.

## Depends on

- step_04_enrich_docs

## Refresh

```bash
dotnet run --project codegen/Novolis.Raylib.Pipeline -- run step_05_verify_docs --force
```
