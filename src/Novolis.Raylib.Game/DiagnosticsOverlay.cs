using System.Drawing;
using Novolis.Raylib.Interact;

namespace Novolis.Raylib.Game;

/// <summary>F3-toggle debug panel with FPS and memory stats.</summary>
public sealed class DiagnosticsOverlay
{
    private readonly SmoothedFps _fps = new();
    private static readonly Color Panel = Color.FromArgb(200, 12, 16, 24);
    private static readonly Color Text = Color.FromArgb(255, 200, 220, 235);

    /// <summary>Whether the overlay is drawn this frame.</summary>
    public bool Visible { get; private set; } = true;

    /// <summary>Panel origin X in screen pixels.</summary>
    public int PanelX { get; set; } = 12;

    /// <summary>Panel origin Y in screen pixels.</summary>
    public int PanelY { get; set; } = 40;

    /// <summary>Toggles overlay visibility.</summary>
    public void Toggle() => Visible = !Visible;

    /// <summary>Toggles when F3 (key code 292) is pressed.</summary>
    /// <param name="ctx">Game context for input polling.</param>
    public void ToggleIfKeyPressed(RayGameContext ctx) =>
        ToggleIfKeyPressed(ctx, (KeyboardKey)292);

    /// <summary>Toggles when the given key is pressed this frame.</summary>
    /// <param name="ctx">Game context for input polling.</param>
    /// <param name="key">Key to watch.</param>
    public void ToggleIfKeyPressed(RayGameContext ctx, KeyboardKey key)
    {
        if (ctx.IsKeyPressed(key))
            Toggle();
    }

    /// <summary>Draws the overlay when <see cref="Visible"/> is true.</summary>
    /// <param name="ctx">Game context for HUD drawing.</param>
    /// <param name="appendLines">Optional extra lines appended after default metrics.</param>
    public void Draw(RayGameContext ctx, Action<FrameDiagnostics, IList<string>>? appendLines = null)
    {
        if (!Visible)
            return;

        _fps.Update(ctx.DeltaSeconds);
        var diag = FrameDiagnostics.Capture(_fps.Value, ctx.DeltaSeconds);
        var lines = new List<string>
        {
            $"FPS {_fps.Value:F0}  ({diag.FrameMilliseconds:F2} ms)",
            $"WS {diag.WorkingSetMegabytes:F0} MB  GC {diag.GcHeapMegabytes:F0} MB",
        };
        appendLines?.Invoke(diag, lines);

        var lineHeight = 20;
        var panelHeight = 12 + lines.Count * lineHeight;
        var panelWidth = 360;
        ctx.HudRect(PanelX, PanelY, panelWidth, panelHeight, Panel);

        var y = PanelY + 8;
        foreach (var line in lines)
        {
            ctx.HudText(line, PanelX + 10, y, 16, Text);
            y += lineHeight;
        }
    }
}
