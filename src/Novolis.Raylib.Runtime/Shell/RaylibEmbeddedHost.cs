using Novolis.Raylib.Abstractions;
using Novolis.Raylib.Audio;
using Novolis.Raylib.Interact;
using Novolis.Raylib.Internal;
using Novolis.Raylib.Logging;
using Novolis.Raylib.Rendering;
using Novolis.Raylib.Timing;
using Novolis.Raylib.Windowing;

namespace Novolis.Raylib.Shell;

/// <summary>
/// Persistent hidden GLFW host for on-demand single-frame rendering (UI bridges).
/// Only one instance may exist per process — GLFW / raylib is not multi-window safe here.
/// </summary>
public sealed class RaylibEmbeddedHost : IDisposable
{
    private static int s_activeHosts;

    private RaylibGlfwProcessSync.LockScope _glfwScope;
    private RaylibEmbeddedOptions _options;
    private byte[] _buffer;
    private bool _initialized;
    private bool _disposed;
    private bool _ownsProcessSlot;

    private RaylibEmbeddedHost(RaylibGlfwProcessSync.LockScope glfwScope, RaylibEmbeddedOptions options, byte[] buffer)
    {
        _glfwScope = glfwScope;
        _options = options;
        _buffer = buffer;
        _ownsProcessSlot = true;
    }

    /// <summary>True when this process already has an embedded GLFW host.</summary>
    public static bool IsProcessHostActive => Volatile.Read(ref s_activeHosts) != 0;

    /// <summary>Creates and initializes a hidden Raylib window (process-wide GLFW lock held for host lifetime).</summary>
    public static RaylibEmbeddedHost Create(RaylibEmbeddedOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return CreateCore(options, static host =>
        {
            Logger.SetTraceLogLevel(TraceLogLevel.Warning);
            AudioDevice.Init();
            try
            {
                host.InitializeWindow();
            }
            catch
            {
                AudioDevice.Close();
                throw;
            }
        });
    }

    /// <summary>Same lock + process-slot contract as <see cref="Create"/>, with a caller-supplied initialize step (tests).</summary>
    internal static RaylibEmbeddedHost CreateCore(RaylibEmbeddedOptions options, Action<RaylibEmbeddedHost> initialize)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(initialize);

        if (Interlocked.CompareExchange(ref s_activeHosts, 1, 0) != 0)
        {
            throw new InvalidOperationException(
                "Only one Raylib embedded GLFW host may run per process.");
        }

        var lockHeld = false;
        RaylibGlfwProcessSync.LockScope glfwScope = default;
        try
        {
            glfwScope = RaylibGlfwProcessSync.Enter(RaylibGlfwProcessSync.EmbeddedHostTimeout);
            lockHeld = true;

            var width = Math.Max(64, options.Width);
            var height = Math.Max(64, options.Height);
            var host = new RaylibEmbeddedHost(
                glfwScope,
                options,
                new byte[width * height * 4]);
            initialize(host);
            return host;
        }
        catch
        {
            if (lockHeld)
                glfwScope.Dispose();

            Interlocked.Exchange(ref s_activeHosts, 0);
            throw;
        }
    }

    /// <summary>Holds the process slot without opening a window (unit tests).</summary>
    internal static IDisposable OccupyProcessSlotForTests()
    {
        if (Interlocked.CompareExchange(ref s_activeHosts, 1, 0) != 0)
        {
            throw new InvalidOperationException(
                "Only one Raylib embedded GLFW host may run per process.");
        }

        return new ProcessSlotReleaser();
    }

    /// <summary>Current framebuffer width in pixels.</summary>
    public int Width => _options.Width;

    /// <summary>Current framebuffer height in pixels.</summary>
    public int Height => _options.Height;

    /// <summary>Resizes the hidden window and reallocates the capture buffer.</summary>
    public void Resize(RaylibEmbeddedOptions options)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(options);

        var width = Math.Max(64, options.Width);
        var height = Math.Max(64, options.Height);
        if (_initialized
            && width == _options.Width
            && height == _options.Height
            && options.TargetFps == _options.TargetFps)
        {
            _options = options;
            return;
        }

        if (_initialized)
        {
            Window.Close();
            _initialized = false;
        }

        _options = new RaylibEmbeddedOptions
        {
            Width = width,
            Height = height,
            WindowTitle = options.WindowTitle,
            TargetFps = options.TargetFps,
            HideWindow = options.HideWindow,
            DisableExitKey = options.DisableExitKey,
        };
        _buffer = new byte[width * height * 4];
        InitializeWindow();
    }

    /// <summary>Draws one frame and invokes <paramref name="onFrame"/> when capture succeeds.</summary>
    public bool TryRenderOneFrame(IRaylibFrameRenderer frameRenderer, Action<RaylibEmbeddedFrame> onFrame)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(frameRenderer);
        ArgumentNullException.ThrowIfNull(onFrame);

        if (!_initialized)
            return false;

        Graphics.BeginDrawing();
        var dt = Time.GetFrameTime();
        var sw = Window.GetScreenWidth();
        var sh = Window.GetScreenHeight();
        frameRenderer.OnFrame(dt, sw, sh);
        Graphics.EndDrawing();

        var ok = ScreenFramebufferCapture.TryCopyFramebufferToRgba(_buffer, sw, sh);
        Window.PollInputEvents();
        if (ok)
            onFrame(new RaylibEmbeddedFrame(_buffer, sw, sh));
        return ok;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        if (_initialized)
        {
            Window.Close();
            _initialized = false;
        }

        AudioDevice.Close();
        _glfwScope.Dispose();
        if (_ownsProcessSlot)
            Interlocked.Exchange(ref s_activeHosts, 0);
    }

    internal void InitializeWindow()
    {
        if (_initialized)
            return;

        if (_options.HideWindow)
            Window.SetConfigFlags(WindowStateFlags.Hidden | WindowStateFlags.Msaa4x);

        Window.Init(_options.Width, _options.Height, _options.WindowTitle);
        if (!Window.IsReady())
        {
            throw new InvalidOperationException(
                "Raylib window is not ready (GLFW or display). Ensure a GPU driver is available.");
        }

        if (_options.DisableExitKey)
            Window.SetExitKey((KeyboardKey)0);

        Time.SetTargetFPS(Math.Clamp(_options.TargetFps, 1, 240));
        _initialized = true;
    }

    private sealed class ProcessSlotReleaser : IDisposable
    {
        private int _released;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _released, 1) != 0)
                return;

            Interlocked.Exchange(ref s_activeHosts, 0);
        }
    }
}
