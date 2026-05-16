namespace Novolis.Raylib.Hosting;

/// <summary>Window and loop configuration for <see cref="RaylibHost"/>.</summary>
public sealed class RaylibHostOptions
{
    /// <summary>Initial window title.</summary>
    public string WindowTitle { get; set; } = "Novolis.Raylib";

    /// <summary>Initial window width in pixels.</summary>
    public int WindowWidth { get; set; } = 1280;

    /// <summary>Initial window height in pixels.</summary>
    public int WindowHeight { get; set; } = 720;

    /// <summary>Target frames per second for the render loop.</summary>
    public int TargetFps { get; set; } = 60;

    /// <summary>How the hosted service drives update vs render phases.</summary>
    public RaylibLoopModel LoopModel { get; set; } = RaylibLoopModel.RenderLoop;

    /// <summary>Fixed timestep for <see cref="IFixedUpdateSystem"/> when using <see cref="RaylibLoopModel.EventLoop"/>.</summary>
    public float FixedTimestepSeconds { get; set; } = 1f / 60f;
}

/// <summary>Hosted loop scheduling model.</summary>
public enum RaylibLoopModel
{
    /// <summary>Render-driven loop: update systems run before each frame draw.</summary>
    RenderLoop,

    /// <summary>Event-driven loop with accumulated fixed updates.</summary>
    EventLoop,
}
