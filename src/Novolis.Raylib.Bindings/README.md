# Novolis.Raylib.Bindings

Generated native interop (`Raylib6Native`, raygui shim) and public façades (`Graphics`, `World3D`, `Input`, …).

- **Do not hand-edit** `*.g.cs` — change manifests under `pipeline/raylib6/` and run codegen.
- **Public types** use BCL where layout matches: `System.Drawing.Color`, `System.Drawing.RectangleF`, `System.Numerics.Vector2` / `Vector3`. Raylib-specific `Camera3D` and opaque `Texture` stay in this package; interop uses internal `RaylibColor` for blittable RGBA.
- **Presets:** `Novolis.Raylib.Colors.RaylibColors` (e.g. `RayWhite`, `DarkGray`).
- **DLL assets** flow via transitive `Novolis.Raylib.Native`.

`Novolis.Raylib.Runtime` builds the window shell and game helpers on top of this package.
