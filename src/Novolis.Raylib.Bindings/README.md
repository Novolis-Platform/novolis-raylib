# Novolis.Raylib.Bindings

Native interop for raylib 6 and the raygui shim (`Raylib6Native`, `RayguiShimExports`, debug hooks).

- **Do not hand-edit** `Interop/*.g.cs` — change manifests under `pipeline/raylib6/` and run codegen.
- **Hand-edited:** blittable `RaylibColor`, `Texture`, `Camera`, input enums, `RaylibColors` presets.
- **Public API** (Graphics, World, Hud, Gui, …) is generated into **`Novolis.Raylib.Runtime`**.

```bash
dotnet run --project codegen/Novolis.Raylib.CodeGen -- generate
```
