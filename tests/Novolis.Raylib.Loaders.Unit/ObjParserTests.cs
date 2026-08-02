using System.Numerics;
using System.Text;
using Novolis.Raylib.Loaders;

namespace Novolis.Raylib.Loaders.Unit;

public class ObjParserTests
{
    [Test]
    public async Task Parses_triangle_face()
    {
        const string obj = """
            v 0 0 0
            v 1 0 0
            v 0 1 0
            f 1 2 3
            """;
        var mesh = ObjParser.ParseTriangleMesh(Encoding.UTF8.GetBytes(obj));
        await Assert.That(mesh.TriangleCount).IsEqualTo(1);
        await Assert.That(mesh.VertexCount).IsEqualTo(3);
    }

    [Test]
    public async Task Parses_quad_face_as_two_triangles()
    {
        const string obj = """
            v 0 0 0
            v 1 0 0
            v 1 1 0
            v 0 1 0
            f 1 2 3 4
            """;
        var mesh = ObjParser.ParseTriangleMesh(Encoding.UTF8.GetBytes(obj));
        await Assert.That(mesh.TriangleCount).IsEqualTo(2);
        await Assert.That(mesh.VertexCount).IsEqualTo(4);
    }

    [Test]
    public async Task Parses_face_with_texture_vertex_indices()
    {
        const string obj = """
            v 0 0 0
            v 1 0 0
            v 0 1 0
            f 1/1 2/2 3/3
            """;
        var mesh = ObjParser.ParseTriangleMesh(Encoding.UTF8.GetBytes(obj));
        await Assert.That(mesh.TriangleCount).IsEqualTo(1);
    }

    [Test]
    public async Task Ignores_comments_and_blank_lines()
    {
        const string obj = """
            # header comment

            v 0 0 0
            v 1 0 0
            v 0 1 0
            f 1 2 3
            """;
        var mesh = ObjParser.ParseTriangleMesh(Encoding.UTF8.GetBytes(obj));
        await Assert.That(mesh.TriangleCount).IsEqualTo(1);
    }

    [Test]
    public async Task Triangulates_ngon_faces()
    {
        var sb = new StringBuilder();
        for (var i = 0; i < 5; i++)
            sb.AppendLine(FormattableString.Invariant($"v {MathF.Cos(i)} {MathF.Sin(i)} 0"));
        sb.AppendLine("f 1 2 3 4 5");
        var mesh = ObjParser.ParseTriangleMesh(Encoding.UTF8.GetBytes(sb.ToString()));
        await Assert.That(mesh.TriangleCount).IsEqualTo(3);
    }
}
