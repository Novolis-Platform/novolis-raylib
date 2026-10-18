<!-- novolis-pkg-brand:start -->
[![Novolis](https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.png)](https://novolis-platform.github.io/.github/novolis-raylib/)

[Novolis](https://github.com/Novolis-Platform) · [Docs](https://novolis-platform.github.io/.github/novolis-raylib/) · [Source](https://github.com/Novolis-Platform/novolis-raylib)
<!-- novolis-pkg-brand:end -->

# Novolis.Raylib.Raygui.Native

Native `novolis_raygui` shim binaries per RID. Referenced transitively by `Novolis.Raylib.Raygui`; do not reference this package directly.

## Install

```bash
dotnet add package Novolis.Raylib
dotnet add package Novolis.Raylib.Raygui
```

## Quick start

Application authors only need `Novolis.Raylib.Raygui`. Native assets copy automatically via `buildTransitive/Novolis.Raylib.Raygui.Native.targets`.

Maintainers refresh natives with:

```bash
dotnet run --project codegen/Novolis.Raylib.Pipeline -- run step_02_native
```

## API

No C# API. Ships `runtimes/{win-x64,linux-x64}/native/novolis_raygui.*`.
macOS (`osx-x64`) is not currently packaged because the raylib 6.2 macOS native build
is not part of the supported maintainer matrix.

## Related

| Package | When to use |
|---------|-------------|
| `Novolis.Raylib.Raygui` | C# raygui widget wrappers |
| `Novolis.Raylib.Native` | Core raylib + imgui natives |

