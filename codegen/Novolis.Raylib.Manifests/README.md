# Novolis.Raylib.Manifests

C#-authoritative binding manifest fragments for `Novolis.Raylib` codegen (interop exports, façades, HUD, raygui/imgui shims).

## Install

```bash
dotnet add package Novolis.Raylib.Manifests
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`), plus `Novolis.CodeGen.Bindings` (pulled transitively).

## Quick start

Reference this package from a **codegen host** or integration test — not from game apps:

```csharp
using Novolis.CodeGen.Bindings;
using Novolis.Raylib.Manifests;

IBindingManifestSource source = RaylibBindingManifestSource.Instance;
var interop = source.GetRequired<InteropExportsFragment>(
    FragmentKind.InteropExports,
    "raylib6");
Console.WriteLine(interop.Imports.Count);
```

Maintainers run the raylib pipeline (`Novolis.Raylib.Pipeline`) which consumes these fragments when generating `Novolis.Raylib.Bindings`.

## Related packages

| Package | When to use |
|---------|-------------|
| `Novolis.CodeGen.Bindings` | Manifest types, emit orchestration, `IBindingManifestSource` |
| `Novolis.Raylib` | Application install — includes generated bindings at runtime |

## More documentation

- [Getting started](https://github.com/Novolis-Platform/novolis-raylib/blob/main/docs/getting-started.md)
- [Binding codegen spec](https://github.com/Novolis-Platform/novolis-codegen/blob/main/docs/specs/binding-codegen-library/initial-idea-v2.md)

## Support

Pre-release (`2026.1.*` on GitHub Packages). Manifest IDs and fingerprints are verified in CI (`step_03_verify_manifest`).
