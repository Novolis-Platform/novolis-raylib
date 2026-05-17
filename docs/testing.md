# Testing

Add `Novolis.Raylib.Testing` to test projects (depends on aggregate `Novolis.Raylib`).

## Golden tests (recommended for visuals)

Golden tests use **internal runtime state** (no environment variables). Enable native offscreen once per assembly:

```csharp
[Before(Assembly)]
public static void EnableNativeTestRuntime() => RaylibTestRuntime.EnableForAssembly();
```

Run a story:

```csharp
var result = RaylibGoldenTest.Run(
    "raylib-golden-smoke-scene",
    new DelegateRaylibFrameRenderer(GoldenScenes.DrawSmokeScene),
    new GoldenRunOptions { TestAssembly = typeof(MyTests).Assembly });

await Assert.That(result.AssertPassed).IsTrue();
```

### QA review bundle

Every run writes a review folder under:

`temp/test-renders/adhoc-runs/{timestamp}_{pid}_{guid}/assemblies/{AssemblyName}/renders/{storyId}/`

| File | Purpose |
|------|---------|
| `index.html` | Two-column QA page: render image(s) and itemized expectations |
| `expectations.md` | Same checklist for agents/CLI |
| `manifest.json` | Machine-readable metadata and hashes |
| `actual.png` | Captured frame |
| `baseline.png` | Copy of committed baseline |

Open `index.html` in a browser for human or agentic visual review, even when SHA256 assert passes in CI.

### Committed baselines

Store under `tests/<Project>/Goldens/{storyId}/`:

- `spec.json` — resolution, `baselineSha256`, and `expectations[]`
- `baseline.png` — reference image

Refresh baselines (explicit test, no env vars):

```csharp
RaylibGoldenTest.Run(storyId, renderer, new GoldenRunOptions
{
    Mode = GoldenRunMode.UpdateBaselines,
    TestAssembly = typeof(MyTests).Assembly,
});
```

See `tests/Novolis.Raylib.Golden/UpdateBaselinesTests` (`[Explicit]`).

### CI

Job `golden-tests` on Windows builds native raylib, runs `[Category("Golden")]`, and uploads `temp/test-renders/` on failure.

## Legacy environment gates

Older helpers still support optional env vars; the **golden framework does not require them**.

| Variable | Purpose |
|----------|---------|
| `NOVOLIS_RAYLIB_OFFSCREEN_TESTS=1` | Legacy offscreen harness |
| `NOVOLIS_RAYLIB_NATIVE_TESTS=1` | Legacy native tests |
| `NOVOLIS_RAYLIB_HEADLESS=1` | Skip window in shell samples |

Prefer `RaylibTestRuntime.EnableForAssembly()` and `RaylibTestRuntime.EnterNativeOffscreen()`.

## Helpers

- `RaylibOffscreenTestHarness` — bounded hidden-window loop
- `RaylibGoldenTest` — golden images + QA HTML reports
- `DeterministicFrameClock` — manual timestep
- `SimulatedInput` — queued keys for tests
- `RaylibTestSession` — scoped native offscreen (runtime state)
- `FramebufferAssert` — PNG SHA256 checks
- `RaylibHostingTestHost` — in-process `IHost`
- `NativeRaylibTestGate` — fail when native offscreen unavailable

E2E (native):

```powershell
./scripts/raylib-e2e.ps1
```
