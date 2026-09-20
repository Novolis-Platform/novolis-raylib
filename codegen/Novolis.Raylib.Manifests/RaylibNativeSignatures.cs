using Novolis.CodeGen.Bindings;

namespace Novolis.Raylib.Manifests;

/// <summary>Typed C ABI signatures used by the curated Raylib, ImGui, and Raygui manifests.</summary>
public static class RaylibNativeSignatures
{
    private static readonly NativeType Color = NativeType.Named("RaylibColor");
    private static readonly NativeType Camera = NativeType.Named("Camera");
    private static readonly NativeType Image = NativeType.Named("Raylib6NativeImage");
    private static readonly NativeType Texture = NativeType.Named("Raylib6NativeTexture");
    private static readonly NativeType Rectangle = NativeType.Named("RectangleF");
    private static readonly NativeType Vector2 = NativeType.Named("Vector2");
    private static readonly NativeType Vector3 = NativeType.Named("Vector3");
    private static readonly NativeType RayguiRectangle = NativeType.Named("RayguiRectangle");

    /// <summary>Void function with no parameters.</summary>
    public static NativeSignature VoidVoid { get; } = NativeSignature.Create(NativeType.Void);

    /// <summary>Boolean function with no parameters.</summary>
    public static NativeSignature BoolVoid { get; } = NativeSignature.Create(NativeType.Boolean);

    /// <summary>Integer function with no parameters.</summary>
    public static NativeSignature IntVoid { get; } = NativeSignature.Create(NativeType.Int32);

    /// <summary>Float function with no parameters.</summary>
    public static NativeSignature FloatVoid { get; } = NativeSignature.Create(NativeType.Float);

    /// <summary>Void function with one integer value.</summary>
    public static NativeSignature VoidInt { get; } = NativeSignature.Create(
        NativeType.Void,
        new NativeParameter("value", NativeType.Int32));

    /// <summary>Void function with unsigned flags.</summary>
    public static NativeSignature VoidUInt { get; } = NativeSignature.Create(
        NativeType.Void,
        new NativeParameter("flags", NativeType.UInt32));

    /// <summary>Boolean function with an integer key.</summary>
    public static NativeSignature BoolInt { get; } = NativeSignature.Create(
        NativeType.Boolean,
        new NativeParameter("key", NativeType.Int32));

    /// <summary>Void function with width, height, and UTF-8 title.</summary>
    public static NativeSignature VoidIntIntUtf8 { get; } = NativeSignature.Create(
        NativeType.Void,
        new NativeParameter("width", NativeType.Int32),
        new NativeParameter("height", NativeType.Int32),
        new NativeParameter("title", NativeType.Utf8String));

    /// <summary>Void function with one color.</summary>
    public static NativeSignature VoidColor { get; } = NativeSignature.Create(
        NativeType.Void,
        new NativeParameter("color", Color));

    /// <summary>Void function for text drawing.</summary>
    public static NativeSignature VoidUtf8IntIntIntColor { get; } = NativeSignature.Create(
        NativeType.Void,
        new NativeParameter("text", NativeType.Utf8String),
        new NativeParameter("posX", NativeType.Int32),
        new NativeParameter("posY", NativeType.Int32),
        new NativeParameter("fontSize", NativeType.Int32),
        new NativeParameter("color", Color));

    /// <summary>Void function with four integers and a color.</summary>
    public static NativeSignature VoidIntIntIntIntColor { get; } = NativeSignature.Create(
        NativeType.Void,
        new NativeParameter("a", NativeType.Int32),
        new NativeParameter("b", NativeType.Int32),
        new NativeParameter("c", NativeType.Int32),
        new NativeParameter("d", NativeType.Int32),
        new NativeParameter("color", Color));

    /// <summary>Void function for a 2D circle.</summary>
    public static NativeSignature VoidIntIntFloatColor { get; } = NativeSignature.Create(
        NativeType.Void,
        new NativeParameter("centerX", NativeType.Int32),
        new NativeParameter("centerY", NativeType.Int32),
        new NativeParameter("radius", NativeType.Float),
        new NativeParameter("color", Color));

    /// <summary>Void function with two position integers.</summary>
    public static NativeSignature VoidIntInt { get; } = NativeSignature.Create(
        NativeType.Void,
        new NativeParameter("posX", NativeType.Int32),
        new NativeParameter("posY", NativeType.Int32));

    /// <summary>Integer text measurement function.</summary>
    public static NativeSignature IntUtf8Int { get; } = NativeSignature.Create(
        NativeType.Int32,
        new NativeParameter("text", NativeType.Utf8String),
        new NativeParameter("fontSize", NativeType.Int32));

    /// <summary>Void function with a rectangle and color.</summary>
    public static NativeSignature VoidRectangleColor { get; } = NativeSignature.Create(
        NativeType.Void,
        new NativeParameter("rec", Rectangle),
        new NativeParameter("color", Color));

    /// <summary>Void function with rectangle coordinates.</summary>
    public static NativeSignature VoidRectInts { get; } = NativeSignature.Create(
        NativeType.Void,
        new NativeParameter("x", NativeType.Int32),
        new NativeParameter("y", NativeType.Int32),
        new NativeParameter("width", NativeType.Int32),
        new NativeParameter("height", NativeType.Int32));

    /// <summary>Void function with a UTF-8 title.</summary>
    public static NativeSignature VoidUtf8 { get; } = NativeSignature.Create(
        NativeType.Void,
        new NativeParameter("title", NativeType.Utf8String));

    /// <summary>Vector2 function with no parameters.</summary>
    public static NativeSignature Vector2Void { get; } = NativeSignature.Create(Vector2);

    /// <summary>Image function with no parameters.</summary>
    public static NativeSignature ImageVoid { get; } = NativeSignature.Create(Image);

    /// <summary>Void function with an image.</summary>
    public static NativeSignature VoidImage { get; } = NativeSignature.Create(
        NativeType.Void,
        new NativeParameter("image", Image));

    /// <summary>Image export-to-memory function.</summary>
    public static NativeSignature NativeIntImageUtf8OutInt { get; } = NativeSignature.Create(
        NativeType.NativeInt,
        new NativeParameter("image", Image),
        new NativeParameter("fileType", NativeType.Utf8String),
        new NativeParameter("fileSize", NativeType.Int32, NativeParameterModifier.Out));

    /// <summary>Void function with an opaque pointer.</summary>
    public static NativeSignature VoidNativeInt { get; } = NativeSignature.Create(
        NativeType.Void,
        new NativeParameter("ptr", NativeType.NativeInt));

    /// <summary>Void function with a camera.</summary>
    public static NativeSignature VoidCamera { get; } = NativeSignature.Create(
        NativeType.Void,
        new NativeParameter("camera", Camera));

    /// <summary>Void function for a cube.</summary>
    public static NativeSignature VoidVector3FloatFloatFloatColor { get; } = NativeSignature.Create(
        NativeType.Void,
        new NativeParameter("position", Vector3),
        new NativeParameter("width", NativeType.Float),
        new NativeParameter("height", NativeType.Float),
        new NativeParameter("length", NativeType.Float),
        new NativeParameter("color", Color));

    /// <summary>Void function between two three-dimensional points.</summary>
    public static NativeSignature VoidVector3Vector3Color { get; } = NativeSignature.Create(
        NativeType.Void,
        new NativeParameter("startPos", Vector3),
        new NativeParameter("endPos", Vector3),
        new NativeParameter("color", Color));

    /// <summary>Void function for a three-dimensional triangle.</summary>
    public static NativeSignature VoidVector3Vector3Vector3Color { get; } = NativeSignature.Create(
        NativeType.Void,
        new NativeParameter("v1", Vector3),
        new NativeParameter("v2", Vector3),
        new NativeParameter("v3", Vector3),
        new NativeParameter("color", Color));

    /// <summary>Void function for a sphere.</summary>
    public static NativeSignature VoidVector3FloatColor { get; } = NativeSignature.Create(
        NativeType.Void,
        new NativeParameter("centerPos", Vector3),
        new NativeParameter("radius", NativeType.Float),
        new NativeParameter("color", Color));

    /// <summary>Void function for a wire sphere.</summary>
    public static NativeSignature VoidVector3FloatIntIntColor { get; } = NativeSignature.Create(
        NativeType.Void,
        new NativeParameter("centerPos", Vector3),
        new NativeParameter("radius", NativeType.Float),
        new NativeParameter("rings", NativeType.Int32),
        new NativeParameter("slices", NativeType.Int32),
        new NativeParameter("color", Color));

    /// <summary>Void function for a cylinder.</summary>
    public static NativeSignature VoidVector3FloatFloatFloatIntColor { get; } = NativeSignature.Create(
        NativeType.Void,
        new NativeParameter("position", Vector3),
        new NativeParameter("radiusTop", NativeType.Float),
        new NativeParameter("radiusBottom", NativeType.Float),
        new NativeParameter("height", NativeType.Float),
        new NativeParameter("slices", NativeType.Int32),
        new NativeParameter("color", Color));

    /// <summary>Void function with integer slices and floating-point spacing.</summary>
    public static NativeSignature VoidIntFloat { get; } = NativeSignature.Create(
        NativeType.Void,
        new NativeParameter("slices", NativeType.Int32),
        new NativeParameter("spacing", NativeType.Float));

    /// <summary>Texture loader function.</summary>
    public static NativeSignature TextureUtf8 { get; } = NativeSignature.Create(
        Texture,
        new NativeParameter("fileName", NativeType.Utf8String));

    /// <summary>Void function with a texture.</summary>
    public static NativeSignature VoidTexture { get; } = NativeSignature.Create(
        NativeType.Void,
        new NativeParameter("texture", Texture));

    /// <summary>Boolean function with a texture.</summary>
    public static NativeSignature BoolTexture { get; } = NativeSignature.Create(
        NativeType.Boolean,
        new NativeParameter("texture", Texture));

    /// <summary>Void texture draw function.</summary>
    public static NativeSignature VoidTextureIntIntColor { get; } = NativeSignature.Create(
        NativeType.Void,
        new NativeParameter("texture", Texture),
        new NativeParameter("posX", NativeType.Int32),
        new NativeParameter("posY", NativeType.Int32),
        new NativeParameter("tint", Color));

    /// <summary>Void textured-rectangle draw function.</summary>
    public static NativeSignature VoidTextureRectangleRectangleVector2FloatColor { get; } = NativeSignature.Create(
        NativeType.Void,
        new NativeParameter("texture", Texture),
        new NativeParameter("source", Rectangle),
        new NativeParameter("dest", Rectangle),
        new NativeParameter("origin", Vector2),
        new NativeParameter("rotation", NativeType.Float),
        new NativeParameter("tint", Color));

    /// <summary>Void three-dimensional plane draw function.</summary>
    public static NativeSignature VoidVector3Vector2Color { get; } = NativeSignature.Create(
        NativeType.Void,
        new NativeParameter("centerPos", Vector3),
        new NativeParameter("size", Vector2),
        new NativeParameter("color", Color));

    /// <summary>Void billboard draw function.</summary>
    public static NativeSignature VoidCameraTextureVector3FloatColor { get; } = NativeSignature.Create(
        NativeType.Void,
        new NativeParameter("camera", Camera),
        new NativeParameter("texture", Texture),
        new NativeParameter("position", Vector3),
        new NativeParameter("scale", NativeType.Float),
        new NativeParameter("tint", Color));

    /// <summary>Void advanced billboard draw function.</summary>
    public static NativeSignature VoidCameraTextureRectangleVector3Vector2Vector2Vector2FloatColor { get; } = NativeSignature.Create(
        NativeType.Void,
        new NativeParameter("camera", Camera),
        new NativeParameter("texture", Texture),
        new NativeParameter("source", Rectangle),
        new NativeParameter("position", Vector3),
        new NativeParameter("up", Vector3),
        new NativeParameter("size", Vector2),
        new NativeParameter("origin", Vector2),
        new NativeParameter("rotation", NativeType.Float),
        new NativeParameter("tint", Color));

    /// <summary>Void ImGui setup function.</summary>
    public static NativeSignature ImguiVoidInt { get; } = NativeSignature.Create(
        NativeType.Void,
        new NativeParameter("value", NativeType.Int32));

    /// <summary>Void ImGui same-line function.</summary>
    public static NativeSignature ImguiVoidFloatFloat { get; } = NativeSignature.Create(
        NativeType.Void,
        new NativeParameter("offsetFromStartX", NativeType.Float),
        new NativeParameter("spacing", NativeType.Float));

    /// <summary>Integer ImGui begin function.</summary>
    public static NativeSignature ImguiIntUtf8OutIntInt { get; } = NativeSignature.Create(
        NativeType.Int32,
        new NativeParameter("name", NativeType.Utf8String),
        new NativeParameter("pOpen", NativeType.Int32, NativeParameterModifier.Out),
        new NativeParameter("flags", NativeType.Int32));

    /// <summary>Integer ImGui UTF-8 function.</summary>
    public static NativeSignature ImguiIntUtf8 { get; } = NativeSignature.Create(
        NativeType.Int32,
        new NativeParameter("label", NativeType.Utf8String));

    /// <summary>Void ImGui UTF-8 function.</summary>
    public static NativeSignature ImguiVoidUtf8 { get; } = NativeSignature.Create(
        NativeType.Void,
        new NativeParameter("text", NativeType.Utf8String));

    /// <summary>Integer ImGui checkbox function.</summary>
    public static NativeSignature ImguiIntUtf8OutInt { get; } = NativeSignature.Create(
        NativeType.Int32,
        new NativeParameter("label", NativeType.Utf8String),
        new NativeParameter("value", NativeType.Int32, NativeParameterModifier.Out));

    /// <summary>Integer ImGui slider function.</summary>
    public static NativeSignature ImguiIntUtf8OutFloatFloatFloat { get; } = NativeSignature.Create(
        NativeType.Int32,
        new NativeParameter("label", NativeType.Utf8String),
        new NativeParameter("value", NativeType.Float, NativeParameterModifier.Out),
        new NativeParameter("min", NativeType.Float),
        new NativeParameter("max", NativeType.Float));

    /// <summary>Void Raygui alpha function.</summary>
    public static NativeSignature RayguiVoidFloat { get; } = NativeSignature.Create(
        NativeType.Void,
        new NativeParameter("alpha", NativeType.Float));

    /// <summary>Void Raygui state function.</summary>
    public static NativeSignature RayguiVoidInt { get; } = NativeSignature.Create(
        NativeType.Void,
        new NativeParameter("state", NativeType.Int32));

    /// <summary>Void Raygui style-set function.</summary>
    public static NativeSignature RayguiVoidIntIntInt { get; } = NativeSignature.Create(
        NativeType.Void,
        new NativeParameter("control", NativeType.Int32),
        new NativeParameter("property", NativeType.Int32),
        new NativeParameter("value", NativeType.Int32));

    /// <summary>Integer Raygui style-get function.</summary>
    public static NativeSignature RayguiIntIntInt { get; } = NativeSignature.Create(
        NativeType.Int32,
        new NativeParameter("control", NativeType.Int32),
        new NativeParameter("property", NativeType.Int32));

    /// <summary>Integer Raygui control function.</summary>
    public static NativeSignature RayguiIntRectangleUtf8 { get; } = NativeSignature.Create(
        NativeType.Int32,
        new NativeParameter("bounds", RayguiRectangle),
        new NativeParameter("text", NativeType.Utf8String));

    /// <summary>Integer Raygui mutable-byte control function.</summary>
    public static NativeSignature RayguiIntRectangleUtf8BytePointer { get; } = NativeSignature.Create(
        NativeType.Int32,
        new NativeParameter("bounds", RayguiRectangle),
        new NativeParameter("text", NativeType.Utf8String),
        new NativeParameter("active", NativeType.BytePointer));

    /// <summary>Integer Raygui mutable-integer control function.</summary>
    public static NativeSignature RayguiIntRectangleUtf8OutInt { get; } = NativeSignature.Create(
        NativeType.Int32,
        new NativeParameter("bounds", RayguiRectangle),
        new NativeParameter("text", NativeType.Utf8String),
        new NativeParameter("active", NativeType.Int32, NativeParameterModifier.Out));

    /// <summary>Integer Raygui floating-point control function.</summary>
    public static NativeSignature RayguiIntRectangleUtf8Utf8OutFloatFloatFloat { get; } = NativeSignature.Create(
        NativeType.Int32,
        new NativeParameter("bounds", RayguiRectangle),
        new NativeParameter("textLeft", NativeType.Utf8String),
        new NativeParameter("textRight", NativeType.Utf8String),
        new NativeParameter("value", NativeType.Float, NativeParameterModifier.Out),
        new NativeParameter("minValue", NativeType.Float),
        new NativeParameter("maxValue", NativeType.Float));
}
