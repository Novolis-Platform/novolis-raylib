using Novolis.CodeGen.Bindings;

namespace Novolis.Raylib.Manifests;

public static partial class RaylibBindingManifests
{
    public static FacadeTypesFragment Gui { get; } = new(
        Id: "gui",
        Types: new FacadeTypeSpec[]
        {
            new(
                Name: "Gui",
                Namespace: "Novolis.Raylib",
                Folder: "Gui",
                TypeSummary: "Dear ImGui immediate-mode UI (cimgui + raylib-cimgui); after scene/HUD, before EndDrawing.",
                Usings: new string[]
                {
                },
                Methods: new FacadeMethodSpec[]
                {
                    new("Setup", "void Setup(bool darkTheme = true)", "GuiControls.Setup(darkTheme)", "Initializes ImGui and the raylib backend (call once after window init)."),
                    new("Shutdown", "void Shutdown()", "GuiControls.Shutdown()", "Shuts down ImGui and releases backend resources."),
                    new("NewFrame", "void NewFrame()", "GuiControls.NewFrame()", "Starts a new ImGui frame (call after BeginDrawing, before widgets)."),
                    new("Render", "void Render()", "GuiControls.Render()", "Submits ImGui draw data to raylib (call after widgets, before EndDrawing)."),
                    new("BeginWindow", "bool BeginWindow(string title, ref bool open)", "GuiControls.BeginWindow(title, ref open)", "Begins an ImGui window; returns false when collapsed."),
                    new("EndWindow", "void EndWindow()", "GuiControls.EndWindow()", "Ends the current ImGui window."),
                    new("Button", "bool Button(string label)", "GuiControls.Button(label)", "ImGui button; returns true when pressed."),
                    new("Text", "void Text(string text)", "GuiControls.Text(text)", "Draws static ImGui text."),
                    new("Checkbox", "bool Checkbox(string label, ref bool value)", "GuiControls.Checkbox(label, ref value)", "ImGui checkbox; returns true when toggled."),
                    new("Slider", "bool Slider(string label, ref float value, float min, float max)", "GuiControls.Slider(label, ref value, min, max)", "ImGui float slider."),
                    new("SameLine", "void SameLine(float offsetFromStartX = 0, float spacing = -1)", "GuiControls.SameLine(offsetFromStartX, spacing)", "Continues layout on the same line."),
                    new("Separator", "void Separator()", "GuiControls.Separator()", "Draws a horizontal separator."),
                }
            ),
        }
    );
}
