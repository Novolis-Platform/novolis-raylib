<!-- novolis-pkg-brand:start -->
[![Novolis](https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.png)](https://novolis-platform.github.io/.github/novolis-raylib/)

[Novolis](https://github.com/Novolis-Platform) · [Docs](https://novolis-platform.github.io/.github/novolis-raylib/) · [Source](https://github.com/Novolis-Platform/novolis-raylib)
<!-- novolis-pkg-brand:end -->

# Novolis.Raylib.Loaders

Wavefront OBJ parsing for indexed triangle meshes (`Novolis.Math.Geometry.TriangleMesh`).

## Install

```bash
dotnet add package Novolis.Raylib.Loaders
```

## Quick start

```csharp
using Novolis.Raylib.Loaders;

var bytes = await File.ReadAllBytesAsync("model.obj");
var mesh = ObjParser.ParseTriangleMesh(bytes);
```

Depends on `Novolis.Math.Geometry` (PackageReference only). No Raylib window required — safe for offline asset pipelines.

## API

| Type | Role |
|------|------|
| `ObjParser` | `ParseTriangleMesh(ReadOnlyMemory<byte>)` → `TriangleMesh` |

## Related

| Package | When to use |
|---------|-------------|
| `Novolis.Raylib.Runtime` | Upload meshes via `World` draw helpers |
| `Novolis.Physics.Collision.Simple` | `BvhStaticWorld` from `TriangleMesh` |

