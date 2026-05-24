using Novolis.Math.Geometry;

namespace Novolis.Raylib.Loaders;

/// <summary>Wavefront OBJ parsing for indexed triangle meshes.</summary>
public static class ObjParser
{
    /// <summary>Parses OBJ bytes into a <see cref="TriangleMesh"/>.</summary>
    public static TriangleMesh ParseTriangleMesh(ReadOnlyMemory<byte> bytes) =>
        ObjHelper.ParseTriangleMesh(bytes.Span);
}
