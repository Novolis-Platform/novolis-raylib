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
}
