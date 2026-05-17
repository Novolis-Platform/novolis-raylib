# Novolis.Raylib.Capture

Optional **per-frame framebuffer streaming** for apps using [`Novolis.Raylib`](../Novolis.Raylib/). Not included in the meta package — add explicitly when you need capture.

```bash
dotnet add package Novolis.Raylib
dotnet add package Novolis.Raylib.Capture
```

## Usage

Call only while a raylib window is active and on the **render thread** (inside your frame loop, after drawing):

```csharp
using Novolis.Raylib.Capture;

var options = new CaptureStreamOptions { CaptureEveryNFrames = 1, MaxBufferedFrames = 64 };
using var session = new FrameCaptureSession(options);

// ... your game loop (RayGame, Hosting, or RaylibRuntimeShell) ...

// Drain captured PNG frames (typically from another thread or after the loop):
var reader = session.Reader;
if (reader is not null)
{
    while (reader.TryRead(out var frame))
        await File.WriteAllBytesAsync($"frame_{frame.FrameIndex:D4}.png", frame.Png);
}
```

Each [`CapturedFrame`](CapturedFrame.cs) carries PNG bytes, dimensions, frame index, and elapsed time since the session started.

## Test projects

Use [`Novolis.Raylib.Testing`](../Novolis.Raylib.Testing/) for golden image tests and offscreen harnesses. That package references Capture transitively for optional streaming during golden runs.

## Notes

- Capture hooks [`RaylibPresentationHooks`](../Novolis.Raylib.Runtime/Presentation/RaylibPresentationHooks.cs) after `Graphics.EndDrawing` (zero cost when no session is active).
- v1 captures **PNG** snapshots via `LoadImageFromScreen`; future versions may add RGBA streaming and a separate recording/MP4 package.
