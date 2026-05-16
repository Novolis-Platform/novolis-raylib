# Novolis.Raylib.Hosting

`IHost` integration with phased game-loop systems. Install **`Novolis.Raylib`** for apps.

## Example

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Novolis.Raylib.Abstractions;
using Novolis.Raylib.Colors;
using Novolis.Raylib.Hosting;
using Novolis.Raylib.Rendering;

var builder = RaylibHost.CreateApplicationBuilder(args);
builder.AddRaylib(o =>
{
    o.WindowTitle = "My App";
    o.WindowWidth = 1280;
    o.WindowHeight = 720;
});
builder.AddRaylibSystem<GameRenderSystem>();
await builder.Build().RunAsync();

sealed class GameRenderSystem : IRenderSystem
{
    public void OnRender(float deltaSeconds, int screenWidth, int screenHeight)
    {
        Graphics.ClearBackground(RaylibColors.RayWhite);
        Graphics.DrawText("Hello Hosting", 16, 16, 24, RaylibColors.DarkGray);
    }
}
```

## Systems

Register types with `AddRaylibSystem<T>()` when they implement one or more of:

| Interface | Phase |
|-----------|--------|
| `IStartupSystem` | Once at start |
| `IFixedUpdateSystem` | Fixed timestep |
| `IUpdateSystem` | Variable update |
| `IRenderSystem` | Draw (inside `BeginDrawing`/`EndDrawing`) |
| `IShutdownSystem` | On exit |

## Options (`RaylibHostOptions`)

- `WindowTitle`, `WindowWidth`, `WindowHeight`, `TargetFps`
- `LoopModel`: `RenderLoop` (default) or `EventLoop`
- `FixedTimestepSeconds` for fixed updates

## Headless

Set `NOVOLIS_RAYLIB_HEADLESS=1` before `RunAsync` to start the host without a blocking raylib window (see sample `HelloHosting`).

## See also

- [Getting started](https://github.com/novolis/novolis-raylib/blob/main/docs/getting-started.md)
- [Game API](https://github.com/novolis/novolis-raylib/blob/main/src/Novolis.Raylib.Game/README.md)
