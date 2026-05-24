# Novolis.Raylib.Native

Transitive NuGet package: **raylib**, **novolis_raylib_trace**, and **novolis_imgui** native binaries per RID.

No C# API. Install **`Novolis.Raylib`** only; native assets copy to the app output via `buildTransitive/Novolis.Raylib.Native.targets`.

RayGui (`novolis_raygui`) ships in **`Novolis.Raylib.Raygui.Native`** when you reference the add-on package.

## RIDs packaged (when prebuilts exist)

| RID | Files |
|-----|--------|
| `win-x64` | `raylib.dll`, `novolis_raylib_trace.dll`, `novolis_imgui.dll` |
| `linux-x64` | `libraylib.so`, `libnovolis_raylib_trace.so`, `libnovolis_imgui.so` |
| `osx-x64` | `libraylib.dylib`, `libnovolis_raylib_trace.dylib`, `libnovolis_imgui.dylib` |

Maintainers run the linear pipeline: `dotnet run --project codegen/Novolis.Raylib.Pipeline -- run step_01_source` then `step_02_native`.

## Troubleshooting

- **DllNotFoundException:** Ensure the app RID matches a packaged runtime and that `raylib`, `novolis_raylib_trace`, and `novolis_imgui` are present in `runtimes/<rid>/native/`.
- **Custom deployment:** Copy the same files from the NuGet package cache or `artifacts/` after `dotnet pack`.

## See also

- [Novolis.Raylib meta package](https://github.com/novolis/novolis-raylib/blob/main/src/Novolis.Raylib/README.md)
- [Novolis.Raylib.Raygui](../Novolis.Raylib.Raygui/) optional raygui add-on
