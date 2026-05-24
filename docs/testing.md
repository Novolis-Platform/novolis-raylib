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

Mark native-dependent tests with `[RunOnlyIfNativeRaylib]` (TUnit skip when native offscreen is unavailable).

### QA review bundle

Every run writes a review folder under:

`temp/test-renders/adhoc-runs/{timestamp}_{pid}_{guid}/assemblies/{AssemblyName}/renders/{storyId}/`

| File | Purpose |
|------|---------|
| `index.html` | Dark QA page (TUnit report styling): dashboard summary + one card per frame with images and checklist |
| `expectations.md` | Same checklist for agents/CLI (`## {frameId}` per frame when multi-frame) |
| `manifest.json` | Machine-readable metadata, per-frame hashes and paths |
| `agent-brief.json` | Compact JSON for agentic review (paths, expectations, hashes) |
| `actual.png` / `{frameId}.actual.png` | Captured frame (v1 / multi-frame) |
| `baseline.png` / `{frameId}.baseline.png` | Copy of committed baseline |

Open `index.html` in a browser for human or agentic visual review, even when SHA256 assert passes in CI.

Local helpers:

```powershell
./scripts/run-golden-tests.ps1
./scripts/open-latest-golden-report.ps1
./scripts/seed-golden-baselines.ps1
```

### Committed baselines

Store under `tests/<Project>/Goldens/{storyId}/`:

- `spec.json` — authoritative metadata: resolution, `baselineSha256`, `expectations[]`, optional `frames[]`
- `baseline.png` — reference image (single-frame v1 stories)
- `{frameId}.png` — per-frame baseline when `frames[]` is present (schema v2)

**Multi-frame `spec.json` (optional):** each entry in `frames[]` has `frameId`, `title`, `caption`, `captureAtFrame`, `maxFrames`, `baselineSha256`, and `expectations[]`. Existing v1 stories without `frames` continue to use `actual.png` / `baseline.png` with no migration required.

[`GoldenScenes`](src/Novolis.Raylib.Testing/Golden/GoldenScenes.cs) provides render delegates only (no duplicated expectations).

Refresh baselines (explicit test or seed console, no env vars):

```csharp
RaylibGoldenTest.Run(storyId, renderer, new GoldenRunOptions
{
    Mode = GoldenRunMode.UpdateBaselines,
    TestAssembly = typeof(MyTests).Assembly,
});
```

See `tests/Novolis.Raylib.Golden/UpdateBaselinesTests` (`[Explicit]`) or `dotnet run --project tests/Novolis.Raylib.Golden.Seed`.

### Golden baseline policy

- **Canonical CI platform:** Windows (`golden-tests` job).
- **Assertion:** exact SHA256 of PNG bytes (no per-pixel tolerance in v1).
- **Stories:** fixed `width` / `height` / `maxFrames`; avoid time-based animation.
- **On hash failure:** open `index.html`, compare the expectations column, then run `seed-golden-baselines.ps1` only if the visual change is intentional.
- **v2 (deferred):** tolerance thresholds and diff images in the HTML report.

### Capture vs debug

| Concept | Location | Purpose |
|---------|----------|---------|
| **Capture** | `Novolis.Raylib.Capture` | General per-frame PNG streaming via `RaylibPresentationHooks` after `EndDrawing` |
| **Golden** | `Novolis.Raylib.Testing.Golden` | Stories, SHA256 baselines, QA HTML reports, `GoldenStreamingCapture` glue |
| **Debug** | `RaylibDebugFrameHooks` | Env-gated DEBUG capture (`NOVOLIS_RAYLIB_DEBUG_CAPTURE`); not used by golden CI |

### Threading and parallel tests

Native / GLFW tests must not run concurrently:

- `[assembly: NotInParallel("raylib-glfw")]` on golden and native test assemblies
- `RaylibGlfwTestSync` global mutex inside `RaylibOffscreenTestHarness`
- `Directory.Build.props` sets `--maximum-parallel-tests 1` for all test projects
- `AsyncLocal` scopes: `RaylibTestRuntimeState`, `RaylibCaptureRuntimeState`
- Streaming capture channel: single writer (render thread), single reader (test thread)

### Harness activation precedence

`RaylibOffscreenTestHarness.IsNativeOffscreenRunRequested()` returns true when, in order:

1. `RaylibTestRuntimeState` (assembly flag or `EnterNativeOffscreen()` scope)
2. `RaylibDebug.Start()` / `NativeOffscreenTestHarnessEnabled`
3. Legacy env pair `NOVOLIS_RAYLIB_OFFSCREEN_TESTS` and `NOVOLIS_RAYLIB_NATIVE_TESTS`

### CI

Job `golden-tests` on Windows builds native raylib, runs `[Category("Golden")]`, and uploads `temp/test-renders/` on failure.

### Artifact mirror (`GoldenArtifactPublisher`)

After `RaylibGoldenTest.Run`, copy the QA bundle into a stable folder (e.g. `raylib-golden-story/` with `{frameId}.png` instead of `{frameId}.actual.png`):

```csharp
var mirror = GoldenArtifactPublisher.Publish(
    result.StoryDirectory!,
    @"temp/test-renders/my-story",
    new GoldenPublishOptions { ReadmeStepSummary = "01-galaxy → 02-production" });
```

Or set `GoldenRunOptions.MirrorPublishDirectory` (and optional `MirrorPublishOptions`) so `RaylibGoldenTest` mirrors automatically; `GoldenTestResult.MirrorPublish` holds `IndexHtmlUri` and paths.

### Run bucket layouts (`IGoldenRunBucketLayout`)

Default output uses `GoldenAdhocRunBucketLayout` (`adhoc-runs/{timestamp}_{pid}_{guid}/...`).

Consumer repos can plug a custom layout (e.g. solution-scoped `solution-runs/{runId}/assemblies/...`):

```csharp
new GoldenRunOptions
{
    TestAssembly = typeof(MyTests).Assembly,
    RunBucketLayout = myLayout,
};
```

`GoldenRenderOutputLayout.Resolve` accepts `IGoldenRunBucketLayout?`; call `GoldenAdhocRunBucketLayout.ResetSharedRun()` between isolated test runs when needed.

### Queued scene renderer

Hosted games often post UI work to a main-thread queue. Wrap the frame renderer:

```csharp
var renderer = new QueuedGoldenStoryRenderer(
    innerRenderer,
    () => hostQueue.Drain(),
    sceneScript);
```

`IGoldenSceneScript.BeginFrame(frameId)` runs after the first drain and before the second (same order as manual drain/setup/drain in product tests).

### Opt-in gate and polling

- `GoldenTestGate.IsOptInEnabled("MY_RAYLIB_GOLDEN")` — true when env is `1` or `true`.
- `GoldenTestPolling.WaitUntil` / `WaitUntilAsync` — blocking wait for session setup.
- `RaylibGoldenTestExtensions.RunAndPublish` — `Run` plus `MirrorPublish` on the result tuple.

## Legacy environment gates

Older helpers still support optional env vars; the **golden framework does not require them**.

| Variable | Purpose |
|----------|---------|
| `NOVOLIS_RAYLIB_OFFSCREEN_TESTS=1` | Legacy offscreen harness |
| `NOVOLIS_RAYLIB_NATIVE_TESTS=1` | Legacy native tests |
| `NOVOLIS_RAYLIB_HEADLESS=1` | Skip window in shell samples |
| `NOVOLIS_RAYLIB_DEBUG_CAPTURE=1` | Debug frame capture (Bindings; unrelated to golden CI) |

Prefer `RaylibTestRuntime.EnableForAssembly()` and `RaylibTestRuntime.EnterNativeOffscreen()`.

## Helpers

- `RaylibOffscreenTestHarness` — bounded hidden-window loop
- `RaylibGoldenTest` — golden images + QA HTML reports
- `DeterministicFrameClock` — manual timestep
- `SimulatedInput` — queued keys for tests
- `RaylibTestSession` — scoped native offscreen (runtime state)
- `FramebufferAssert` — PNG SHA256 checks
- `RaylibHostingTestHost` — in-process `IHost`
- `NativeRaylibTestGate.IsAvailable` — probe native offscreen availability

E2E (native):

```powershell
./scripts/raylib-e2e.ps1
```

## Coverage

Line coverage targets **hand-written C#** in `src/` and `codegen/`. Generated `*.g.cs` files are excluded (guarded by codegen drift and golden tests instead).

| Excluded | Included |
|----------|----------|
| `**/*.g.cs` | Bindings hand types, Runtime shell/debug, Game, Hosting, Capture, Testing helpers, CodeGen + Pipeline |
| `samples/` | CLI logic via unit tests |
| `Novolis.Raylib.Native` (no C#) | |

Local run:

```powershell
./scripts/run-coverage.ps1
```

Output: `artifacts/coverage/index.html` and `coverage/baseline-summary.json`.

Configuration: `build/Novolis.Raylib.Coverage.props`, `coverage.runsettings`, and `dotnet test --coverage` (Microsoft Testing Platform / dotnet-coverage).

CI job `coverage` (Windows) runs tests with coverage and fails below the current floor (**48%** line rate on hand-written assemblies). Ratchet milestones: **70%** → **85%** → **95%** (raise `COVERAGE_THRESHOLD` in CI as coverage improves).

Optional `[ExcludeFromCodeCoverage]` requires a justification comment in source.
