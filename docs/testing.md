# Testing

Add `Novolis.Raylib.Testing` to test projects (depends on aggregate `Novolis.Raylib`).

## Environment gates

| Variable | Purpose |
|----------|---------|
| `NOVOLIS_RAYLIB_OFFSCREEN_TESTS=1` | Enable offscreen harness |
| `NOVOLIS_RAYLIB_NATIVE_TESTS=1` | Enable native smoke / harness |
| `NOVOLIS_RAYLIB_HEADLESS=1` | Skip window in shell samples |

## Helpers

- `RaylibOffscreenTestHarness` — bounded hidden-window loop
- `DeterministicFrameClock` — manual timestep
- `SimulatedInput` — queued keys for tests
- `RaylibTestSession` — scoped env + debug gates
- `FramebufferAssert` — PNG SHA256 checks
- `RaylibHostingTestHost` — in-process `IHost`

E2E (native):

```powershell
./scripts/raylib-e2e.ps1
```
