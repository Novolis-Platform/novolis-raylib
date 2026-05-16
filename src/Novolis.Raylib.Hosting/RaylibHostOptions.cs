namespace Novolis.Raylib.Hosting;

/// <summary>Window and loop configuration for <see cref="RaylibHost"/>.</summary>
public sealed class RaylibHostOptions
{
    public string WindowTitle { get; set; } = "Novolis.Raylib";

    public int WindowWidth { get; set; } = 1280;

    public int WindowHeight { get; set; } = 720;

    public int TargetFps { get; set; } = 60;

    public RaylibLoopModel LoopModel { get; set; } = RaylibLoopModel.RenderLoop;

    public float FixedTimestepSeconds { get; set; } = 1f / 60f;
}

public enum RaylibLoopModel
{
    RenderLoop,
    EventLoop,
}
