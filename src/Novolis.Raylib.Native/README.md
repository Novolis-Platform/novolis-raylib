# Novolis.Raylib.Native

Transitive NuGet package: **raylib**, **novolis_raylib_trace**, and **novolis_imgui** native binaries per RID.

No C# API. Native assets copy to the app output via `buildTransitive/Novolis.Raylib.Native.targets`.

## Install

```bash
dotnet add package Novolis.Raylib
```

## Quick start

Do not reference this package directly. Add **`Novolis.Raylib`** to your app; native runtimes restore automatically for your RID.

RayGui (`novolis_raygui`) ships in **`Novolis.Raylib.Raygui.Native`** when you reference the add-on package.

## RIDs packaged

| RID | Files |
|-----|--------|
| `win-x64` | `raylib.dll`, `novolis_raylib_trace.dll`, `novolis_imgui.dll` |
| `linux-x64` | `libraylib.so`, `libnovolis_raylib_trace.so`, `libnovolis_imgui.so` |
| `osx-x64` | `libraylib.dylib`, `libnovolis_raylib_trace.dylib`, `libnovolis_imgui.dylib` |

Maintainers: `dotnet run --project codegen/Novolis.Raylib.Pipeline -- run step_01_source` then `step_02_native`.

## Troubleshooting

- **DllNotFoundException:** Ensure the app RID matches a packaged runtime and natives are present under `runtimes/<rid>/native/`.

## Related

| Package | When to use |
|---------|-------------|
| `Novolis.Raylib` | Meta package for application authors |
| `Novolis.Raylib.Raygui.Native` | Optional raygui natives |
