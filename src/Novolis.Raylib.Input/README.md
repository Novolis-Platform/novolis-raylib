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
input.OnKeyPress(args => { /* ... */ });
input.Start();
```

Provide a platform implementation (for example SharpHook-backed) in the host app or a future provider package.
