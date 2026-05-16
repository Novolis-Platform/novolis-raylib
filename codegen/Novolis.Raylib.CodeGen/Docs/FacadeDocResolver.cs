using System.Text.RegularExpressions;

namespace Novolis.Raylib.CodeGen;

internal static partial class FacadeDocResolver
{
    public const string RaylibCheatsheetUrl = "https://www.raylib.com/cheatsheet/cheatsheet.html";

    private static readonly Regex RaylibNativeCall = new(
        @"Raylib6Native\.(\w+)\s*\(",
        RegexOptions.Compiled);

    private static readonly Regex GuiControlsCall = new(
        @"GuiControls\.(\w+)\s*\(",
        RegexOptions.Compiled);

    private static readonly Regex GraphicsCall = new(
        @"Graphics\.(\w+)\s*\(",
        RegexOptions.Compiled);

    private static readonly Regex TexturesCall = new(
        @"Textures\.(\w+)\s*\(",
        RegexOptions.Compiled);

    private static readonly Dictionary<string, string> DefaultTypeSummaries = new(StringComparer.Ordinal)
    {
        ["Graphics"] = "2D drawing and framebuffer operations (raylib drawing module).",
        ["Window"] = "Window lifecycle, configuration, and display state.",
        ["Input"] = "Keyboard, mouse, and cursor input.",
        ["World"] = "3D scene rendering; call Begin with a Camera, then draw, then End.",
        ["Textures"] = "Texture load, draw, and unload helpers.",
        ["Time"] = "Frame timing and target FPS.",
        ["AudioDevice"] = "Audio device initialization.",
        ["Hud"] = "Screen-space 2D overlay drawn after World.End (same frame, before EndDrawing).",
        ["Gui"] = "raygui immediate-mode widgets (UTF-8); draw after scene and HUD, before EndDrawing.",
    };

    private static readonly Dictionary<string, string> HudSemanticSummaries = new(StringComparer.Ordinal)
    {
        ["Clear"] = "Clears the framebuffer with a color (screen-space HUD; uses ClearBackground).",
        ["Text"] = "Draws text in screen coordinates (HUD overlay).",
        ["MeasureText"] = "Measures text width in pixels for HUD layout.",
        ["Rect"] = "Filled axis-aligned rectangle in screen space.",
        ["RectOutline"] = "Outlined axis-aligned rectangle in screen space.",
        ["RectF"] = "Filled rectangle from a RectangleF in screen space.",
        ["Line"] = "Line segment in screen space.",
        ["Circle"] = "Filled circle in screen space.",
        ["DrawTexture"] = "Draws a texture at screen position with tint.",
        ["DrawTexturePro"] = "Draws a texture region into a destination rectangle (HUD sprites).",
        ["Fps"] = "Draws frames-per-second counter at screen position.",
    };

    private static readonly Dictionary<string, string> GuiSemanticSummaries = new(StringComparer.Ordinal)
    {
        ["Enable"] = "Enables raygui controls (global state).",
        ["Disable"] = "Disables raygui controls (global state).",
        ["Lock"] = "Locks raygui controls (global state).",
        ["Unlock"] = "Unlocks raygui controls (global state).",
        ["SetAlpha"] = "Sets global raygui alpha.",
        ["LoadStyleDefault"] = "Loads default raygui style.",
        ["Button"] = "raygui button; returns true when pressed.",
        ["Label"] = "raygui static label.",
        ["Panel"] = "raygui panel with title.",
        ["GroupBox"] = "raygui group box with title.",
        ["Toggle"] = "raygui toggle control.",
        ["CheckBox"] = "raygui checkbox.",
        ["ComboBox"] = "raygui combo box.",
        ["Slider"] = "raygui slider with label.",
        ["SliderBar"] = "raygui slider bar with label.",
        ["ProgressBar"] = "raygui progress bar with label.",
    };

    private static readonly Dictionary<string, string> GuiNativeNames = new(StringComparer.Ordinal)
    {
        ["Enable"] = "GuiEnable",
        ["Disable"] = "GuiDisable",
        ["Lock"] = "GuiLock",
        ["Unlock"] = "GuiUnlock",
        ["SetAlpha"] = "GuiSetAlpha",
        ["LoadStyleDefault"] = "GuiLoadStyleDefault",
        ["Button"] = "GuiButton",
        ["Label"] = "GuiLabel",
        ["Panel"] = "GuiPanel",
        ["GroupBox"] = "GuiGroupBox",
        ["Toggle"] = "GuiToggle",
        ["CheckBox"] = "GuiCheckBox",
        ["ComboBox"] = "GuiComboBox",
        ["Slider"] = "GuiSlider",
        ["SliderBar"] = "GuiSliderBar",
        ["ProgressBar"] = "GuiProgressBar",
    };

    public static string? ResolveTypeSummary(FacadeTypeDefinition type)
    {
        if (!string.IsNullOrWhiteSpace(type.TypeSummary))
            return type.TypeSummary.Trim();

        return DefaultTypeSummaries.TryGetValue(type.Name, out var summary) ? summary : null;
    }

    public static string ResolveMethodSummary(
        FacadeTypeDefinition type,
        FacadeMethodDefinition method,
        IReadOnlyDictionary<string, string> raylibComments,
        IReadOnlyDictionary<string, string> rayguiComments)
    {
        if (!string.IsNullOrWhiteSpace(method.Summary))
            return method.Summary.Trim();

        if (type.Name == "Hud" && HudSemanticSummaries.TryGetValue(method.Name, out var hudSummary))
            return hudSummary;

        if (type.Name == "Gui" && GuiSemanticSummaries.TryGetValue(method.Name, out var guiSummary))
            return guiSummary;

        var nativeName = TryResolveNativeSymbol(type, method);
        if (nativeName is not null)
        {
            if (raylibComments.TryGetValue(nativeName, out var raylibComment) && !string.IsNullOrWhiteSpace(raylibComment))
                return raylibComment;

            if (rayguiComments.TryGetValue(nativeName, out var rayguiComment) && !string.IsNullOrWhiteSpace(rayguiComment))
                return rayguiComment;
        }

        var displayName = nativeName ?? method.Name;
        return $"Wraps raylib {displayName}. See {RaylibCheatsheetUrl}.";
    }

    public static string? TryResolveNativeSymbol(FacadeTypeDefinition type, FacadeMethodDefinition method)
    {
        var body = method.Body;

        var raylibMatch = RaylibNativeCall.Match(body);
        if (raylibMatch.Success)
            return raylibMatch.Groups[1].Value;

        if (type.Name == "Gui")
        {
            var guiMatch = GuiControlsCall.Match(body);
            if (guiMatch.Success && GuiNativeNames.TryGetValue(guiMatch.Groups[1].Value, out var guiNative))
                return guiNative;
        }

        if (type.Name == "Hud")
        {
            var gfxMatch = GraphicsCall.Match(body);
            if (gfxMatch.Success)
                return gfxMatch.Groups[1].Value;

            var texMatch = TexturesCall.Match(body);
            if (texMatch.Success)
                return texMatch.Groups[1].Value switch
                {
                    "Draw" => "DrawTexture",
                    "DrawPro" => "DrawTexturePro",
                    _ => texMatch.Groups[1].Value,
                };
        }

        return null;
    }
}
