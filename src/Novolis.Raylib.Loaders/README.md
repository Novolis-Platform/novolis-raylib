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
