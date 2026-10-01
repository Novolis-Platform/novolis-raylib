<!-- novolis-pkg-brand:start -->
[![Novolis](https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.png)](https://novolis-platform.github.io/.github/novolis-raylib/)

[Novolis](https://github.com/Novolis-Platform) · [Docs](https://novolis-platform.github.io/.github/novolis-raylib/) · [Source](https://github.com/Novolis-Platform/novolis-raylib)
<!-- novolis-pkg-brand:end -->

# Novolis.Raylib.Input

Input capture abstractions for Raylib hosts (keyboard/mouse). No dependency on Simulation.

## Install

```bash
dotnet add package Novolis.Raylib.Input
```

## Quick start

```csharp
using Novolis.Raylib.Input;

IInputSource input = new NullInputSource(); // headless tests
input.OnKeyPress += args => { /* KeyCode */ };
input.Start();
```

Provide a platform implementation (for example SharpHook-backed) in the host app or a future provider package.

## API

| Type | Role |
|------|------|
| `IInputSource` | `OnMouseMove`, `OnMouseClick`, `OnKeyPress`, `OnKeyRelease`; `Start`, `Stop` |
| `NullInputSource` | No-op for headless/CI |
| `MouseEventArgs` | `X`, `Y`, `Button` |
| `KeyboardEventArgs` | `KeyCode` |

## Related

| Package | When to use |
|---------|-------------|
| `Novolis.Raylib.Runtime` | Built-in `Input` façade inside the shell loop |
| `Novolis.Raylib.Bindings` | `KeyboardKey`, `MouseButton` enums |

