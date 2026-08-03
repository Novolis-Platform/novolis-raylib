<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-raylib">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
<!-- novolis-pkg-brand:end -->

# Novolis.Raylib.Capture

Optional **per-frame framebuffer streaming** for apps using [`Novolis.Raylib`](../Novolis.Raylib/). Not included in the meta package — add explicitly when you need capture.

## Install

```bash
dotnet add package Novolis.Raylib
dotnet add package Novolis.Raylib.Capture
```

## Quick start

Call only while a raylib window is active and on the **render thread** (inside your frame loop, after drawing):

```csharp
using Novolis.Raylib.Capture;

var options = new CaptureStreamOptions { CaptureEveryNFrames = 1, MaxBufferedFrames = 64 };
using var session = new FrameCaptureSession(options);

// ... your game loop ...

while (session.Reader?.TryRead(out var frame) == true)
    await File.WriteAllBytesAsync($"frame_{frame.FrameIndex:D4}.png", frame.Png);
```

## API

| Type | Role |
|------|------|
| `CaptureStreamOptions` | `CaptureEveryNFrames`, `MaxBufferedFrames` |
| `CapturedFrame` | `FrameIndex`, `Width`, `Height`, `Png`, `Elapsed` |
| `FrameCaptureSession` | `Reader`; implements `IDisposable` |
| `FrameCapturePipeline` | Static `Start`/`Stop` for global hook |
| `RaylibCaptureRuntimeState` | `IsStreamingActive`, `Enter(options)` scope |

Capture hooks presentation after `Graphics.EndDrawing` (zero cost when no session is active).

## Related

| Package | When to use |
|---------|-------------|
| `Novolis.Raylib.Testing` | Golden image tests (references Capture transitively) |
| `Novolis.Raylib.Runtime` | Shell loop and draw hooks |

