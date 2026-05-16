# Novolis.Raylib.Native

Transitive NuGet package: **raylib** and **novolis_raygui** native binaries per RID (`.dll` / `.so` / `.dylib`).

This package contains **no C# code**. Generated P/Invoke and façades live in **`Novolis.Raylib.Bindings`**. Hand-crafted shell/helpers live in **`Novolis.Raylib.Runtime`**.

Do **not** reference this package directly. Add **`Novolis.Raylib`** only; native assets flow via `Novolis.Raylib.Native` and `buildTransitive/Novolis.Raylib.Native.targets`.
