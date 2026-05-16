# Novolis.Raylib.Runtime

Manifest-generated raylib façades plus HUD/GUI layers, with a hand-crafted window shell.

## Generated (`*.g.cs` — do not edit)

| Area | Types | Manifest |
|------|--------|----------|
| Core | `Graphics`, `World`, `Textures`, `Window`, `Input`, `Time`, `AudioDevice` | `facades.manifest.json` |
| HUD (2D overlay) | `Hud` | `hud.manifest.json` |
| GUI (raygui widgets) | `Gui` | `gui.manifest.json` |

Regenerate: `dotnet run --project codegen/Novolis.Raylib.CodeGen -- generate`

## Hand-crafted

- `Shell/` — window loop, frame renderer
- `Debug/`, `Logging/` — trace and diagnostics
- `Gui/GuiControls.cs` — UTF-8 marshalling for generated `Gui`
- `RayguiShimHost.cs` — loads `novolis_raygui.dll`

Interop (`Raylib6Native`, …) lives in **`Novolis.Raylib.Bindings`**.

## Usage sketch

```csharp
Graphics.BeginDrawing();
World.Begin(camera);
// ... 3D scene ...
World.End();
Hud.Text("Score 42", 12, 12, 20, RaylibColors.White);
Gui.Button(new RectangleF(8, 8, 120, 28), "Play");
Graphics.EndDrawing();
```

Install **`Novolis.Raylib`** for apps; this package is pulled in transitively.
