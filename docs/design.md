# Design

Multi-package Raylib 6 + raygui bindings, game API, hosting, and testing.

Published docs: [https://novolis-platform.github.io/.github/novolis-raylib/](https://novolis-platform.github.io/.github/novolis-raylib/)

## Layer placement

**Raylib** island — never references Simulation; apps wire Raylib + Simulation.

## Goals

- Keep public APIs documented and packable as `Novolis.*` on GitHub Packages (when applicable).
- Prefer BCL types and existing Novolis packages over parallel abstractions.
- Document restore and ProjectReference-mode builds without local NuGet folder feeds.

## Non-goals

- Local NuGet folder feeds or committed cross-repo `ProjectReference` into sibling checkouts.
- Avalonia package references outside `Novolis.Avalonia.*`.
- Upward spine dependencies (e.g. Math → Simulation).

## Packages

- `Novolis.Raylib`
- `Novolis.Raylib.Abstractions`
- `Novolis.Raylib.Bindings`
- `Novolis.Raylib.Capture`
- `Novolis.Raylib.Game`
- `Novolis.Raylib.Hosting`
- `Novolis.Raylib.Input`
- `Novolis.Raylib.Loaders`
- `Novolis.Raylib.Manifests`
- `Novolis.Raylib.Native`
- `Novolis.Raylib.Raygui`
- `Novolis.Raylib.Raygui.Native`
- `Novolis.Raylib.Runtime`
- `Novolis.Raylib.Testing`

## Topics

- `dotnet`
- `raylib`
- `gamedev`
- `novolis`
