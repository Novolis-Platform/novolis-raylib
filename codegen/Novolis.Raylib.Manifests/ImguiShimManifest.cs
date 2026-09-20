using Novolis.CodeGen.Bindings;

namespace Novolis.Raylib.Manifests;

public static partial class RaylibBindingManifests
{
    public static ShimExportsFragment ImguiShim { get; } = new(
        Id: "imgui",
        SchemaVersion: 1,
        Header: "codegen/vendor/raylib-cimgui/cimgui-1.92.1-docking/cimgui.h",
        Description: "Curated novolis_imgui shim exports (cimgui + raylib-cimgui).",
        ModuleFileName: "novolis_imgui",
        Exports: new ShimExportSpec[]
        {
            new("novolis_rlimgui_setup", RaylibNativeSignatures.ImguiVoidInt),
            new("novolis_rlimgui_shutdown", RaylibNativeSignatures.VoidVoid),
            new("novolis_rlimgui_begin", RaylibNativeSignatures.VoidVoid),
            new("novolis_rlimgui_end", RaylibNativeSignatures.VoidVoid),
            new("novolis_igBegin", RaylibNativeSignatures.ImguiIntUtf8OutIntInt),
            new("novolis_igEnd", RaylibNativeSignatures.VoidVoid),
            new("novolis_igButton", RaylibNativeSignatures.ImguiIntUtf8),
            new("novolis_igText", RaylibNativeSignatures.ImguiVoidUtf8),
            new("novolis_igCheckbox", RaylibNativeSignatures.ImguiIntUtf8OutInt),
            new("novolis_igSliderFloat", RaylibNativeSignatures.ImguiIntUtf8OutFloatFloatFloat),
            new("novolis_igSameLine", RaylibNativeSignatures.ImguiVoidFloatFloat),
            new("novolis_igSeparator", RaylibNativeSignatures.VoidVoid),
        }
    );
}
