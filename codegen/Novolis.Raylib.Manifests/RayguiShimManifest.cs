using Novolis.CodeGen.Bindings;

namespace Novolis.Raylib.Manifests;

public static partial class RaylibBindingManifests
{
    public static ShimExportsFragment RayguiShim { get; } = new(
        Id: "raygui",
        SchemaVersion: 1,
        Header: "codegen/vendor/raygui-6/raygui.h",
        Description: "Curated RAYGUIAPI exports bound via novolis_raygui shim (function pointers). Template ids map to emit logic in generate-raygui-interop.cs.",
        ModuleFileName: "novolis_raygui",
        Exports: new ShimExportSpec[]
        {
            new("GuiEnable", "void_void"),
            new("GuiDisable", "void_void"),
            new("GuiLock", "void_void"),
            new("GuiUnlock", "void_void"),
            new("GuiSetAlpha", "void_float"),
            new("GuiSetState", "void_int"),
            new("GuiGetState", "int_void"),
            new("GuiSetStyle", "void_int_int_int"),
            new("GuiGetStyle", "int_int_int"),
            new("GuiLoadStyleDefault", "void_void"),
            new("GuiPanel", "int_rect_utf8"),
            new("GuiGroupBox", "int_rect_utf8"),
            new("GuiLine", "int_rect_utf8"),
            new("GuiLabel", "int_rect_utf8"),
            new("GuiStatusBar", "int_rect_utf8"),
            new("GuiDummyRec", "int_rect_utf8"),
            new("GuiButton", "int_rect_utf8"),
            new("GuiLabelButton", "int_rect_utf8"),
            new("GuiToggle", "int_rect_utf8_ptrbyte"),
            new("GuiCheckBox", "int_rect_utf8_ptrbyte"),
            new("GuiComboBox", "int_rect_utf8_ptrint"),
            new("GuiSlider", "int_rect_utf8_utf8_ptrfloat_float_float"),
            new("GuiSliderBar", "int_rect_utf8_utf8_ptrfloat_float_float"),
            new("GuiProgressBar", "int_rect_utf8_utf8_ptrfloat_float_float"),
        }
    );
}
