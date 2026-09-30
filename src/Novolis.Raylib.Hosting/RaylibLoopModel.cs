using Novolis.Raylib.Abstractions;

namespace Novolis.Raylib.Hosting;

/// <summary>Hosted loop scheduling model.</summary>
public enum RaylibLoopModel
{
    /// <summary>Render-driven loop: update systems run before each frame draw.</summary>
    RenderLoop,

    /// <summary>Event-driven loop with accumulated fixed updates.</summary>
    EventLoop,
}
