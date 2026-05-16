# Novolis.Raylib.Runtime

Manifest-generated raylib façades plus HUD/GUI layers, with a hand-crafted window shell.

## Frame order

Each frame inside the shell loop:

1. `Graphics.BeginDrawing()`
2. `World.Begin(camera)` … 3D draws … `World.End()`
3. `Hud.*` — screen-space 2D overlay
4. `Gui.*` — raygui widgets (UTF-8 via shim)
5. `Graphics.EndDrawing()`

`RayGame` and `IRenderSystem` callbacks run between steps 1 and 5; you choose where to call World/Hud/Gui.

## Generated (`*.g.cs` — do not edit)

| Area | Types | Manifest |
|------|--------|----------|
| Core | `Graphics`, `World`, `Textures`, `Window`, `Input`, `Time`, `AudioDevice` | `facades.manifest.json` |
| HUD (2D overlay) | `Hud` | `hud.manifest.json` |
| GUI (raygui widgets) | `Gui` | `gui.manifest.json` |

Regenerate: `dotnet run --project codegen/Novolis.Raylib.CodeGen -- generate`

IntelliSense summaries come from manifest `summary` fields (enriched from raylib headers via `enrich-docs`).

## Hand-crafted

- `Shell/` — `RaylibRuntimeShell`, window loop
- `Debug/`, `Logging/` — trace and diagnostics
- `Gui/GuiControls.cs` — UTF-8 marshalling for generated `Gui`

Interop (`Raylib6Native`, …) lives in **`Novolis.Raylib.Bindings`**.

## Shell

```csharp
RaylibRuntimeShell.RunShellFrame("Title", 800, 600, new MyRenderer());
```

Set `NOVOLIS_RAYLIB_HEADLESS=1` to skip the window (CI).

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

## See also

- [Game](https://github.com/novolis/novolis-raylib/blob/main/src/Novolis.Raylib.Game/README.md)
- [Hosting](https://github.com/novolis/novolis-raylib/blob/main/src/Novolis.Raylib.Hosting/README.md)
