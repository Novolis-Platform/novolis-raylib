using Novolis.CodeGen.Bindings;

namespace Novolis.Raylib.Manifests;

public static partial class RaylibBindingManifests
{
    public static FacadeTypesFragment RayguiFacade { get; } = new(
        Id: "raygui",
        Types: new FacadeTypeSpec[]
        {
            new(
                Name: "RayGui",
                Namespace: "Novolis.Raylib",
                Folder: "RayGui",
                TypeSummary: "raygui immediate-mode widgets (UTF-8); optional add-on; draw after scene and HUD, before EndDrawing.",
                Usings: new string[]
                {
                    "System.Drawing",
                },
                Methods: new FacadeMethodSpec[]
                {
                    new("Enable", "void Enable()", "RayGuiControls.Enable()", "Enables raygui controls (global state)."),
                    new("Disable", "void Disable()", "RayGuiControls.Disable()", "Disables raygui controls (global state)."),
                    new("Lock", "void Lock()", "RayGuiControls.Lock()", "Locks raygui controls (global state)."),
                    new("Unlock", "void Unlock()", "RayGuiControls.Unlock()", "Unlocks raygui controls (global state)."),
                    new("SetAlpha", "void SetAlpha(float alpha)", "RayGuiControls.SetAlpha(alpha)", "Sets global raygui alpha."),
                    new("LoadStyleDefault", "void LoadStyleDefault()", "RayGuiControls.LoadStyleDefault()", "Loads default raygui style."),
                    new("Button", "bool Button(RectangleF bounds, string label)", "RayGuiControls.Button(bounds, label)", "raygui button; returns true when pressed."),
                    new("Label", "void Label(RectangleF bounds, string text)", "RayGuiControls.Label(bounds, text)", "raygui static label."),
                    new("Panel", "bool Panel(RectangleF bounds, string title)", "RayGuiControls.Panel(bounds, title)", "raygui panel with title."),
                    new("GroupBox", "void GroupBox(RectangleF bounds, string title)", "RayGuiControls.GroupBox(bounds, title)", "raygui group box with title."),
                    new("Toggle", "bool Toggle(RectangleF bounds, string label, ref bool active)", "RayGuiControls.Toggle(bounds, label, ref active)", "raygui toggle control."),
                    new("CheckBox", "bool CheckBox(RectangleF bounds, string label, ref bool active)", "RayGuiControls.CheckBox(bounds, label, ref active)", "raygui checkbox."),
                    new("ComboBox", "bool ComboBox(RectangleF bounds, string text, ref int active)", "RayGuiControls.ComboBox(bounds, text, ref active)", "raygui combo box."),
                    new("Slider", "bool Slider(RectangleF bounds, string label, ref float value, float min, float max)", "RayGuiControls.Slider(bounds, label, ref value, min, max)", "raygui slider with label."),
                    new("SliderBar", "bool SliderBar(RectangleF bounds, string label, ref float value, float min, float max)", "RayGuiControls.SliderBar(bounds, label, ref value, min, max)", "raygui slider bar with label."),
                    new("ProgressBar", "bool ProgressBar(RectangleF bounds, string label, ref float value, float min, float max)", "RayGuiControls.ProgressBar(bounds, label, ref value, min, max)", "raygui progress bar with label."),
                }
            ),
        }
    );
}
