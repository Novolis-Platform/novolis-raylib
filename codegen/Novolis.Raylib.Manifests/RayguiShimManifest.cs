using Novolis.CodeGen.Bindings;

namespace Novolis.Raylib.Manifests;

public static partial class RaylibBindingManifests
{
    public static ShimExportsFragment RayguiShim { get; } = new(
        Id: "raygui",
        SchemaVersion: 1,
        Header: "codegen/vendor/raygui-6/raygui.h",
        Description: "Curated RAYGUIAPI exports bound via the novolis_raygui function-pointer shim.",
        ModuleFileName: "novolis_raygui",
        Exports: new ShimExportSpec[]
        {
            new("GuiEnable", RaylibNativeSignatures.VoidVoid),
            new("GuiDisable", RaylibNativeSignatures.VoidVoid),
            new("GuiLock", RaylibNativeSignatures.VoidVoid),
            new("GuiUnlock", RaylibNativeSignatures.VoidVoid),
            new("GuiSetAlpha", RaylibNativeSignatures.RayguiVoidFloat),
            new("GuiSetState", RaylibNativeSignatures.RayguiVoidInt),
            new("GuiGetState", RaylibNativeSignatures.IntVoid),
            new("GuiSetStyle", RaylibNativeSignatures.RayguiVoidIntIntInt),
            new("GuiGetStyle", RaylibNativeSignatures.RayguiIntIntInt),
            new("GuiLoadStyleDefault", RaylibNativeSignatures.VoidVoid),
            new("GuiPanel", RaylibNativeSignatures.RayguiIntRectangleUtf8),
            new("GuiGroupBox", RaylibNativeSignatures.RayguiIntRectangleUtf8),
            new("GuiLine", RaylibNativeSignatures.RayguiIntRectangleUtf8),
            new("GuiLabel", RaylibNativeSignatures.RayguiIntRectangleUtf8),
            new("GuiStatusBar", RaylibNativeSignatures.RayguiIntRectangleUtf8),
            new("GuiDummyRec", RaylibNativeSignatures.RayguiIntRectangleUtf8),
            new("GuiButton", RaylibNativeSignatures.RayguiIntRectangleUtf8),
            new("GuiLabelButton", RaylibNativeSignatures.RayguiIntRectangleUtf8),
            new("GuiToggle", RaylibNativeSignatures.RayguiIntRectangleUtf8BytePointer),
            new("GuiCheckBox", RaylibNativeSignatures.RayguiIntRectangleUtf8BytePointer),
            new("GuiComboBox", RaylibNativeSignatures.RayguiIntRectangleUtf8OutInt),
            new("GuiSlider", RaylibNativeSignatures.RayguiIntRectangleUtf8Utf8OutFloatFloatFloat),
            new("GuiSliderBar", RaylibNativeSignatures.RayguiIntRectangleUtf8Utf8OutFloatFloatFloat),
            new("GuiProgressBar", RaylibNativeSignatures.RayguiIntRectangleUtf8Utf8OutFloatFloatFloat),
        },
        EmbeddedTypes:
        [
            new EmbeddedTypeSpec(
                "RayguiRectangle",
                [
                    new EmbeddedFieldSpec("X", "float"),
                    new EmbeddedFieldSpec("Y", "float"),
                    new EmbeddedFieldSpec("Width", "float"),
                    new EmbeddedFieldSpec("Height", "float"),
                ]),
        ]
    );
}
