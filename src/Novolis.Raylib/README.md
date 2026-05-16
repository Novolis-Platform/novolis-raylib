# Novolis.Raylib

One-line install for the full Novolis Raylib stack: [raylib](https://www.raylib.com/) 6, raygui, generated façades, Game and Hosting entry APIs.

## Install

```bash
dotnet add package Novolis.Raylib
```

**Prerequisites:** .NET 10 SDK. Native binaries are included per RID (see [Novolis.Raylib.Native](https://github.com/novolis/novolis-raylib/blob/main/src/Novolis.Raylib.Native/README.md)).

## Choose an API

| Goal | Package (transitive) | Entry point |
|------|----------------------|-------------|
| Jam game / tutorial | `Novolis.Raylib.Game` | `RayGame.Run` |
| DI, phased systems | `Novolis.Raylib.Hosting` | `RaylibHost` + `AddRaylib` |
| Full control | `Novolis.Raylib.Runtime` | `Graphics`, `World`, `Hud`, `Gui`, `RaylibRuntimeShell` |

Do **not** reference `Novolis.Raylib.Native` or `Novolis.Raylib.Abstractions` directly unless you extend the stack.

## Quick start

```csharp
using Novolis.Raylib.Colors;
using Novolis.Raylib.Game;

RayGame.Run("Demo", 800, 600, ctx =>
{
    ctx.Clear(RaylibColors.RayWhite);
    ctx.Text("Hello", 12, 12, 20, RaylibColors.DarkGray);
});
```

## Frame order (Runtime)

Within each frame: `Graphics.BeginDrawing` → `World.Begin` … `World.End` → `Hud` / `Gui` → `Graphics.EndDrawing`.

## Headless / CI

Set `NOVOLIS_RAYLIB_HEADLESS=1` to skip the blocking window loop (samples and tests).

## More documentation

- [Getting started](https://github.com/novolis/novolis-raylib/blob/main/docs/getting-started.md)
- [Game API](https://github.com/novolis/novolis-raylib/blob/main/src/Novolis.Raylib.Game/README.md)
- [Hosting API](https://github.com/novolis/novolis-raylib/blob/main/src/Novolis.Raylib.Hosting/README.md)
- [Runtime façades](https://github.com/novolis/novolis-raylib/blob/main/src/Novolis.Raylib.Runtime/README.md)

## Support

Pre-release (`0.1.x-alpha`). Windows is the primary CI platform; Linux/macOS RIDs are packaged when vendor prebuilts exist.
