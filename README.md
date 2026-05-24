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

## Packages

All publishable packages live under `src/`. Install **`Novolis.Raylib`** for games and apps; it pulls the rest transitively. Do **not** reference `Novolis.Raylib.Native` or `Novolis.Raylib.Abstractions` directly unless you have a special reason.

```mermaid
flowchart TB
  meta["Novolis.Raylib<br/><i>meta — install this</i>"]
  game[Novolis.Raylib.Game]
  hosting[Novolis.Raylib.Hosting]
  runtime[Novolis.Raylib.Runtime]
  bindings[Novolis.Raylib.Bindings]
  abstractions[Novolis.Raylib.Abstractions]
  native["Novolis.Raylib.Native<br/><i>DLLs only, no C#</i>"]
  testing["Novolis.Raylib.Testing<br/><i>test projects</i>"]
  capture["Novolis.Raylib.Capture<br/><i>optional</i>"]

  meta --> game
  meta --> hosting
  meta --> runtime
  meta --> abstractions
  meta --> native
  game --> runtime
  hosting --> runtime
  runtime --> bindings
  runtime --> abstractions
  bindings --> native
  abstractions --> native
  testing --> meta
  capture --> runtime
```

| Package | Role | Depends on |
|---------|------|------------|
| **Novolis.Raylib** | Meta package — one install for the full stack | Game, Hosting, Runtime, Abstractions, Native |
| **Novolis.Raylib.Game** | `RayGame.Run` jam API | Runtime |
| **Novolis.Raylib.Hosting** | `IHost` + phased systems (`IRenderSystem`, …) | Runtime |
| **Novolis.Raylib.Runtime** | Generated façades (`Graphics`, `World`, `Hud`, `Gui`, …), window shell, logging, debug | Bindings, Abstractions, Native (assets) |
| **Novolis.Raylib.Bindings** | Generated P/Invoke + shared types (`Camera`, `Texture`, …) | Native (assets) |
| **Novolis.Raylib.Abstractions** | Frame/shell contracts (transitive) | Native (assets) |
| **Novolis.Raylib.Native** | `raylib` + `novolis_raygui` binaries per RID (no managed code) | — |
| **Novolis.Raylib.Capture** | Per-frame framebuffer streaming (`FrameCaptureSession`) | Runtime |
| **Novolis.Raylib.Testing** | Offscreen harness, golden QA, deterministic clock (tests only) | Novolis.Raylib, Capture |

**Optional capture** (not in the meta package):

```bash
dotnet add package Novolis.Raylib.Capture
```

**Native assets:** `Novolis.Raylib.Native` packs prebuilt `raylib` from `codegen/pipeline/raylib6/steps/step_01_source/artifacts/` plus shims from `step_02_native/artifacts/` (built via maintainer pipeline). That is separate from **`Novolis.Raylib.Bindings`**, which holds the C# interop.

**Not published:** Roslyn codegen under `codegen/` (see [docs/codegen.md](docs/codegen.md)).

## Getting started

See [docs/getting-started.md](docs/getting-started.md) for choosing Game vs Hosting vs Runtime, frame order, and samples.

## Quick start (Game)

```csharp
using Novolis.Raylib.Colors;
using Novolis.Raylib.Game;

RayGame.Run("Demo", 800, 600, ctx =>
{
    ctx.Clear(RaylibColors.RayWhite);
    ctx.Text("Hello", 12, 12, 20, RaylibColors.DarkGray);
});
```

## Maintainer pipeline

```bash
dotnet run --project codegen/Novolis.Raylib.Pipeline -- run maintainer
dotnet run --project codegen/Novolis.Raylib.Pipeline -- run agent-verify
dotnet build Novolis.Raylib.slnx
```

See [docs/codegen.md](docs/codegen.md), [docs/testing.md](docs/testing.md), and [agentic-tools/README.md](agentic-tools/README.md) (AI agent workflows).
