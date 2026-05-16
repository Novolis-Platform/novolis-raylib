# Novolis.Raylib.Testing

Optional test helpers for projects that use **`Novolis.Raylib`**. Reference this package **only from test projects**.

## Environment gates

| Variable | Purpose |
|----------|---------|
| `NOVOLIS_RAYLIB_OFFSCREEN_TESTS=1` | Enable offscreen harness |
| `NOVOLIS_RAYLIB_NATIVE_TESTS=1` | Enable native smoke / harness |
| `NOVOLIS_RAYLIB_HEADLESS=1` | Skip blocking window in shell samples |
| `NOVOLIS_RAYLIB_DEBUG_CAPTURE=1` | Debug framebuffer capture |

## Minimal harness

```csharp
using Novolis.Raylib.Abstractions;
using Novolis.Raylib.Rendering;
using Novolis.Raylib.Testing;

var result = RaylibOffscreenTestHarness.Run(new MyRenderer(), new RaylibOffscreenTestOptions
{
    FrameCount = 2,
    Width = 320,
    Height = 240,
});
```

Set `NOVOLIS_RAYLIB_OFFSCREEN_TESTS=1` and `NOVOLIS_RAYLIB_NATIVE_TESTS=1` before running native/offscreen tests locally or in CI.

## Helpers

| Type | Role |
|------|------|
| `RaylibOffscreenTestHarness` | Bounded hidden-window loop |
| `DeterministicFrameClock` | Manual timestep |
| `SimulatedInput` | Queued keys |
| `FramebufferAssert` | PNG SHA256 checks |
| `RaylibHostingTestHost` | In-process `IHost` |

Full details: [docs/testing.md](https://github.com/novolis/novolis-raylib/blob/main/docs/testing.md).

## E2E (maintainers)

```powershell
./scripts/raylib-e2e.ps1
```
