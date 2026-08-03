<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-raylib">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
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

