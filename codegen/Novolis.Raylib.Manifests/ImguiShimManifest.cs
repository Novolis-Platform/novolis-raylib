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
            new("novolis_rlimgui_setup", "void_int"),
            new("novolis_rlimgui_shutdown", "void_void"),
            new("novolis_rlimgui_begin", "void_void"),
            new("novolis_rlimgui_end", "void_void"),
            new("novolis_igBegin", "int_utf8_ptrint_int"),
            new("novolis_igEnd", "void_void"),
            new("novolis_igButton", "int_utf8"),
            new("novolis_igText", "void_utf8"),
            new("novolis_igCheckbox", "int_utf8_ptrint"),
            new("novolis_igSliderFloat", "int_utf8_ptrfloat_float_float"),
            new("novolis_igSameLine", "void_float_float"),
            new("novolis_igSeparator", "void_void"),
        }
    );
}
