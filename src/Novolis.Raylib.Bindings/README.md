# Novolis.Raylib.Bindings

Generated P/Invoke for raylib 6 and the raygui shim (`Raylib6Native`, `RayguiShimExports`, debug hooks).

**Application authors:** use **`Novolis.Raylib.Runtime`** façades (`Graphics`, `World`, `Hud`, `Gui`). This package is for advanced scenarios and is pulled in transitively.

## Maintainer rules

- **Do not hand-edit** `Interop/*.g.cs` — change manifests under `pipeline/raylib6/` and run codegen.
- **Hand-edited public types:** `Camera`, `Texture`, input enums, `RaylibColors` presets.

```bash
dotnet run --project codegen/Novolis.Raylib.CodeGen -- generate
```

## Public API surface

| Type | Role |
|------|------|
| `Camera`, `Texture` | Blittable-friendly wrappers used by façades |
| `RaylibColors` | Common `Color` presets |
| `KeyboardKey`, `MouseButton` | Input enums |

Generated interop types are `internal`.

## See also

- [Runtime](https://github.com/novolis/novolis-raylib/blob/main/src/Novolis.Raylib.Runtime/README.md)
- [Codegen docs](https://github.com/novolis/novolis-raylib/blob/main/docs/codegen.md)
