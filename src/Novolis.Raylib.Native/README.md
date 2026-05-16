# Novolis.Raylib.Native

Transitive NuGet package: **raylib** and **novolis_raygui** native binaries per RID.

No C# API. Install **`Novolis.Raylib`** only; native assets copy to the app output via `buildTransitive/Novolis.Raylib.Native.targets`.

## RIDs packaged (when prebuilts exist)

| RID | Files |
|-----|--------|
| `win-x64` | `raylib.dll`, `novolis_raygui.dll` |
| `linux-x64` | `libraylib.so`, `libnovolis_raygui.so` |
| `osx-x64` | `libraylib.dylib`, `libnovolis_raygui.dylib` |

Maintainers build the raygui shim: `dotnet run pipeline/raylib6/run.cs native`.

## Troubleshooting

- **DllNotFoundException:** Ensure the app RID matches a packaged runtime and that both `raylib` and `novolis_raygui` are present in `runtimes/<rid>/native/`.
- **Custom deployment:** Copy the same files from the NuGet package cache or `artifacts/` after `dotnet pack`.

## See also

- [Novolis.Raylib meta package](https://github.com/novolis/novolis-raylib/blob/main/src/Novolis.Raylib/README.md)
