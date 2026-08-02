# step_02a_android

NDK-build **static** `libraylib.a` (`PLATFORM=Android`, `arm64-v8a`) and thin host `libnovolis_raylib_android.so`, then stage both into:

`src/Novolis.Raylib.Native/runtimes/android-arm64/native/`

## Requirements

- Android NDK (`ANDROID_NDK_HOME` / `ANDROID_NDK_ROOT`, or `%LOCALAPPDATA%\Android\Sdk\ndk\<version>`)
- CMake + Ninja on `PATH`
- Network (clones raylib tag from `versions.json` `raylibGitTag` on first run)

## Depends on

- None (clones its own raylib sources under `artifacts/`)

## Refresh

```powershell
dotnet run --project d:\novolis\novolis-raylib\codegen\Novolis.Raylib.Pipeline -- run step_02a_android --force
```

Not part of the default `maintainer` profile (NDK required). CI packs checked-in `runtimes/` without running this step.
