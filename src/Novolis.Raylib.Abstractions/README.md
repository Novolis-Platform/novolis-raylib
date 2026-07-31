# Novolis.Raylib.Abstractions

Transitive contracts for Raylib shell hosts: frame renderers, lifecycle systems, and invalidation hooks.

Do **not** reference this package directly unless you extend the Raylib stack. Application authors install **`Novolis.Raylib`**.

## Install

```bash
dotnet add package Novolis.Raylib
```

## Quick start

Implement `IRaylibFrameRenderer` in a custom host, or use [Novolis.Raylib.Game](../Novolis.Raylib.Game/README.md) / [Novolis.Raylib.Hosting](../Novolis.Raylib.Hosting/README.md) instead.

```csharp
public sealed class MyRenderer : IRaylibFrameRenderer
{
    public void OnFrame(float deltaSeconds, int screenWidth, int screenHeight) { /* draw */ }
}
```

## API

| Type | Role |
|------|------|
| `IRaylibFrameRenderer` | `OnFrame(delta, width, height)` |
| `IRaylibShellRuntime` | `RunShellFrame(title, renderer, cancellationToken?)` |
| `IUpdateSystem` | Variable-rate update hook |
| `IFixedUpdateSystem` | Fixed-timestep update hook |
| `IRenderSystem` | Render hook with screen dimensions |
| `IStartupSystem` | Shell startup hook |
| `IShutdownSystem` | Shell shutdown hook |
| `IRaylibInvalidationSource` | `IsInvalidated`, `ClearInvalidation()` |

## Related

| Package | When to use |
|---------|-------------|
| `Novolis.Raylib.Runtime` | Generated `Graphics`, `World`, `Hud`, `Gui` façades |
| `Novolis.Raylib.Game` | ECS-style game loop on top of the shell |
