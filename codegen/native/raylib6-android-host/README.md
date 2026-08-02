# raylib6-android-host

Thin NativeActivity host that **static-links** `libraylib.a` (PLATFORM_ANDROID) and implements `main()` for raylib’s `android_main`.

## Build

Requires Android NDK + Ninja. Prefer the pipeline step:

```powershell
dotnet run --project d:\novolis\novolis-raylib\codegen\Novolis.Raylib.Pipeline -- run step_02a_android --force
```

Outputs stage to `src/Novolis.Raylib.Native/runtimes/android-arm64/native/`.
