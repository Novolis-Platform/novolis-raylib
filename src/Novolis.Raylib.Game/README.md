# Novolis.Raylib.Game

Low-ceremony API for jams and tutorials (included transitively by **`Novolis.Raylib`**).

## Install

```bash
dotnet add package Novolis.Raylib
```

## Quick start

```csharp
using Novolis.Raylib.Colors;
using Novolis.Raylib.Game;

RayGame.Run("My Game", 1280, 720,
    initialize: ctx => { /* once */ },
    update: ctx =>
    {
        ctx.Clear(RaylibColors.RayWhite);
        ctx.BeginWorld(Camera.Perspective(
            new(4, 4, 4), Vector3.Zero, Vector3.UnitY));
        ctx.DrawShipBox(Vector3.Zero, new(1, 0.5f, 2), RaylibColors.Red);
        ctx.EndWorld();
        ctx.Text($"FPS ~ {1f / ctx.DeltaSeconds:F0}", 8, 8, 18, RaylibColors.DarkGray);
    });
```

## RayGameContext cheat sheet

| Member | Purpose |
|--------|---------|
| `Width`, `Height`, `DeltaSeconds` | Screen size and frame time |
| `Clear`, `Text`, `Rect` | 2D HUD helpers |
| `BeginWorld` / `EndWorld` | 3D pass |
| `DrawShipBox`, `DrawBolt`, … | Common 3D placeholders |
| `IsKeyDown`, `IsKeyPressed`, `MouseDelta` | Input |

## Headless

When `NOVOLIS_RAYLIB_HEADLESS=1`, the shell skips window creation (useful in CI). See [Runtime shell](https://github.com/novolis/novolis-raylib/blob/main/src/Novolis.Raylib.Runtime/README.md).

## See also

- [Novolis.Raylib meta package](https://github.com/novolis/novolis-raylib/blob/main/src/Novolis.Raylib/README.md)
- [Hosting](https://github.com/novolis/novolis-raylib/blob/main/src/Novolis.Raylib.Hosting/README.md) for `IHost` integration
