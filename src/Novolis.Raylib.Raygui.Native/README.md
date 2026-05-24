# Novolis.Raylib.Raygui.Native

Native `novolis_raygui` shim binaries per RID. Referenced transitively by `Novolis.Raylib.Raygui`; do not reference this package directly.

## Install

```bash
dotnet add package Novolis.Raylib
dotnet add package Novolis.Raylib.Raygui
```

## Quick start

Application authors only need `Novolis.Raylib.Raygui`. Maintainers refresh natives with:

```bash
dotnet run --project codegen/Novolis.Raylib.Pipeline -- run step_02_native
```
