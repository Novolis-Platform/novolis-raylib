using System.Drawing;
using Novolis.Raylib.Interact;

namespace Novolis.Raylib.Game;

/// <summary>F3-toggle debug panel with FPS and memory stats.</summary>
public sealed class DiagnosticsOverlay
{
    private readonly SmoothedFps _fps = new();
    private static readonly Color Panel = Color.FromArgb(200, 12, 16, 24);
    private static readonly Color Text = Color.FromArgb(255, 200, 220, 235);

    public bool Visible { get; private set; } = true;

    public int PanelX { get; set; } = 12;

    public int PanelY { get; set; } = 40;

    public void Toggle() => Visible = !Visible;

    public void ToggleIfKeyPressed(RayGameContext ctx) =>
        ToggleIfKeyPressed(ctx, (KeyboardKey)292);

    public void ToggleIfKeyPressed(RayGameContext ctx, KeyboardKey key)
    {
        if (ctx.IsKeyPressed(key))
            Toggle();
    }

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
