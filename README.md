# Novolis.Raylib

Multi-package .NET bindings for [raylib](https://www.raylib.com/) 6 + raygui.

## Install

```bash
dotnet add package Novolis.Raylib
```

Test projects:

```bash
dotnet add package Novolis.Raylib.Testing
```

## Quick start (Game)

```csharp
using Novolis.Raylib.Colors;
using Novolis.Raylib.Game;

RayGame.Run("Demo", 800, 600, ctx =>
{
    ctx.Clear(Color.RayWhite);
    ctx.Text("Hello", 12, 12, 20, Color.DarkGray);
});
```

## Maintainer pipeline

```bash
dotnet run pipeline/raylib6/run.cs all   # fetch + native + codegen
dotnet build Novolis.Raylib.slnx
./scripts/raylib-codegen-check.ps1
```

See [docs/codegen.md](docs/codegen.md) and [docs/testing.md](docs/testing.md).
