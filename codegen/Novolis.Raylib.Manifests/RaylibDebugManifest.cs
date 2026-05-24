using Novolis.CodeGen.Bindings;

namespace Novolis.Raylib.Manifests;

public static partial class RaylibBindingManifests
{
    public static DebugConfigFragment RaylibDebug { get; } = new(
        Id: "raylib-debug",
        SchemaVersion: 1,
        Description: "Debug-only frame hooks; native calls reference names from raylib-exports.manifest.json imports.",
        NotifyAfterNativeCall: "EndDrawing",
        FrameHubNotifyAfter: "EndDrawing",
        CaptureEnvVar: "NOVOLIS_RAYLIB_DEBUG_CAPTURE",
        CapturePngFileType: ".png",
        Symbols: new(
            LoadImageFromScreen: "LoadImageFromScreen",
            ExportImageToMemory: "ExportImageToMemory",
            UnloadImage: "UnloadImage",
            MemFree: "MemFree"
        )
    );
}
