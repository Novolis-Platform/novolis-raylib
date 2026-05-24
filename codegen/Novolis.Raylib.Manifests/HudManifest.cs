using Novolis.CodeGen.Bindings;

namespace Novolis.Raylib.Manifests;

public static partial class RaylibBindingManifests
{
    public static FacadeTypesFragment Hud { get; } = new(
        Id: "hud",
        Types: new FacadeTypeSpec[]
        {
            new(
                Name: "Hud",
                Namespace: "Novolis.Raylib",
                Folder: "Hud",
                TypeSummary: "Screen-space 2D overlay drawn after World.End (same frame, before EndDrawing).",
                Usings: new string[]
                {
                    "System.Drawing",
                    "System.Numerics",
                    "Novolis.Raylib.Rendering",
                },
                Methods: new FacadeMethodSpec[]
                {
                    new("Clear", "void Clear(Color color)", "Graphics.ClearBackground(color)", "Clears the framebuffer with a color (screen-space HUD; uses ClearBackground)."),
                    new("Text", "void Text(string text, int x, int y, int fontSize, Color color)", "Graphics.DrawText(text, x, y, fontSize, color)", "Draws text in screen coordinates (HUD overlay)."),
                    new("MeasureText", "int MeasureText(string text, int fontSize)", "Graphics.MeasureText(text, fontSize)", "Measures text width in pixels for HUD layout."),
                    new("Rect", "void Rect(int x, int y, int width, int height, Color color)", "Graphics.DrawRectangle(x, y, width, height, color)", "Filled axis-aligned rectangle in screen space."),
                    new("RectOutline", "void RectOutline(int x, int y, int width, int height, Color color)", "Graphics.DrawRectangleLines(x, y, width, height, color)", "Outlined axis-aligned rectangle in screen space."),
                    new("RectF", "void RectF(RectangleF rect, Color color)", "Graphics.DrawRectangleRec(rect, color)", "Filled rectangle from a RectangleF in screen space."),
                    new("Line", "void Line(int x1, int y1, int x2, int y2, Color color)", "Graphics.DrawLine(x1, y1, x2, y2, color)", "Line segment in screen space."),
                    new("Circle", "void Circle(int centerX, int centerY, float radius, Color color)", "Graphics.DrawCircle(centerX, centerY, radius, color)", "Filled circle in screen space."),
                    new("DrawTexture", "void DrawTexture(Texture texture, int x, int y, Color tint)", "Textures.Draw(texture, x, y, tint)", "Draws a texture at screen position with tint."),
                    new("DrawTexturePro", "void DrawTexturePro(Texture texture, RectangleF source, RectangleF dest, Vector2 origin, float rotation, Color tint)", "Textures.DrawPro(texture, source, dest, origin, rotation, tint)", "Draws a texture region into a destination rectangle (HUD sprites)."),
                    new("Fps", "void Fps(int x, int y)", "Graphics.DrawFPS(x, y)", "Draws frames-per-second counter at screen position."),
                }
            ),
        }
    );
}
