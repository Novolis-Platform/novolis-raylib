# Novolis.Raylib.Bindings

Generated P/Invoke for raylib 6 and the raygui shim, plus hand-edited public wrappers.

**Application authors:** use **`Novolis.Raylib.Runtime`** façades (`Graphics`, `World`, `Hud`, `Gui`). This package is for advanced scenarios and is pulled in transitively.

## Install

```bash
dotnet add package Novolis.Raylib
```

## Quick start

Regenerate bindings from manifests (maintainers only):

```bash
dotnet run --project codegen/Novolis.Raylib.Pipeline -- run generate
```

## API

| Type | Role |
|------|------|
| `Camera` | Perspective/orthographic camera struct |
| `Texture` | `Id`, `Width`, `Height`, `IsValid` |
| `RaylibColors` | Common color presets |
| `RaylibVector3` | `ForwardFromYawPitch` |
| `KeyboardKey`, `MouseButton` | Input enums aligned with raylib |
| `Utf8StringMarshaller` | UTF-8 interop helper |

Generated interop types (`Raylib6Native`, …) are `internal`.

## Maintainer rules

- **Do not hand-edit** `Interop/*.g.cs` — change manifests under `codegen/pipeline/raylib6/` and run codegen.

## Related

| Package | When to use |
|---------|-------------|
| `Novolis.Raylib.Runtime` | Application-facing façades |
| [Codegen docs](https://github.com/novolis/novolis-raylib/blob/main/docs/codegen.md) | Manifest and pipeline reference |
