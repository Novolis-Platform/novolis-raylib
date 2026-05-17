# Novolis.Raylib.Testing

Optional test helpers for projects that use **`Novolis.Raylib`**. Reference this package **only from test projects**.

## Golden tests (preferred for visuals)

No environment variables. Enable native offscreen per assembly:

```csharp
RaylibTestRuntime.EnableForAssembly();
```

```csharp
var result = RaylibGoldenTest.Run(
    "raylib-golden-smoke-scene",
    new DelegateRaylibFrameRenderer(DrawScene),
    new GoldenRunOptions { TestAssembly = typeof(MyTests).Assembly });
```

Each run writes `temp/test-renders/adhoc-runs/.../renders/{storyId}/index.html` with render images and an itemized QA expectations column. See [docs/testing.md](../../docs/testing.md).

## Native offscreen harness

```csharp
using var scope = RaylibTestRuntime.EnterNativeOffscreen();
var result = RaylibOffscreenTestHarness.Run(renderer, new RaylibOffscreenTestOptions
{
    MaxFrames = 4,
    Width = 320,
    Height = 240,
    CaptureLastFramePng = true,
});
```

Legacy env vars (`NOVOLIS_RAYLIB_OFFSCREEN_TESTS`, `NOVOLIS_RAYLIB_NATIVE_TESTS`) still work for older tests.

## Helpers

| Type | Role |
|------|------|
| `RaylibGoldenTest` | Golden SHA256 + QA HTML reports |
| `GoldenArtifactPublisher` | Mirror QA bundle to stable `{frameId}.png` folders |
| `IGoldenRunBucketLayout` | Pluggable `temp/test-renders/...` run folder layout |
| `QueuedGoldenStoryRenderer` | Drain main-thread queue around scene setup |
| `IGoldenSceneScript` | Per-frame navigation/setup before capture |
| `GoldenTestGate` | Opt-in env gate (`1` / `true`) |
| `GoldenTestPolling` | `WaitUntil` / `WaitUntilAsync` for hosted setup |
| `RaylibGoldenTestExtensions` | `RunAndPublish` convenience |
| `RaylibTestRuntime` | Scoped native test state (no env vars) |
| `RaylibOffscreenTestHarness` | Bounded hidden-window loop |
| `RaylibTestSession` | Scoped `EnterNativeOffscreen()` |
| `DeterministicFrameClock` | Manual timestep |
| `FramebufferAssert` | PNG SHA256 checks |
| `RunOnlyIfNativeRaylib` | TUnit skip when native unavailable |
| `NativeRaylibTestGate` | Probe native offscreen availability |
| `RaylibHostingTestHost` | In-process `IHost` |

Full details: [docs/testing.md](../../docs/testing.md).

## E2E (maintainers)

```powershell
./scripts/raylib-e2e.ps1
```
