# Codegen

Bindings are generated at **library build** from JSON manifests under `pipeline/raylib6/`.

- `codegen/Novolis.Raylib.CodeGen` — Roslyn CLI (`generate`, `verify`, `suggest-raylib`, `hooks list`; not published)
- Output is committed under `src/Novolis.Raylib.Bindings/**/*.g.cs` with `ManifestSha256` headers
- CI runs `scripts/raylib-codegen-check.ps1` to fail on drift

Regenerate locally:

```bash
dotnet run --project codegen/Novolis.Raylib.CodeGen -- generate
```
