<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-raylib">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
<!-- novolis-pkg-brand:end -->

# Novolis.Raylib.Native

Transitive NuGet package: **raylib**, **novolis_raylib_trace**, and **novolis_imgui** native binaries per RID (desktop), plus **android-arm64** static raylib + NativeActivity host.

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
| `android-arm64` | `libraylib.a` (static), `libnovolis_raylib_android.so` (NativeActivity host; static-links raylib) |

### Android notes

Upstream raylib Android is **static-link oriented** (`android_main` → user `main`). Do not expect desktop-style managed `InitWindow` / `LibraryImport("raylib")` for the game loop yet.

`net*-android` apps that consume this package (without excluding `buildTransitive`) get `libnovolis_raylib_android.so` as an `AndroidNativeLibrary` (`arm64-v8a`). Point `NativeActivity` / `android.app.lib_name` at **`novolis_raylib_android`**.

Maintainers (NDK + Ninja required):

```powershell
dotnet run --project d:\novolis\novolis-raylib\codegen\Novolis.Raylib.Pipeline -- run step_02a_android --force
```

Desktop maintainers: `dotnet run --project codegen/Novolis.Raylib.Pipeline -- run step_01_source` then `step_02_native`.

## Troubleshooting

- **DllNotFoundException:** Ensure the app RID matches a packaged runtime and natives are present under `runtimes/<rid>/native/`.
- **Android UnsatisfiedLinkError:** Confirm the host `.so` is packed into the APK and `android.app.lib_name` is `novolis_raylib_android`.

## Related

| Package | When to use |
|---------|-------------|
| `Novolis.Raylib` | Meta package for application authors |
| `Novolis.Raylib.Raygui.Native` | Optional raygui natives |

