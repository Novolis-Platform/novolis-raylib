using System.Reflection;
using System.Text;
using Novolis.Raylib.Testing;
using Novolis.Raylib.Testing.Golden;

namespace Novolis.Raylib.Testing.Unit;

public sealed class GoldenPublishResultTests
{
    [Test]
    public async Task IndexHtmlUri_is_file_scheme()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "novolis-testing-unit", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        var indexPath = Path.Combine(tempDir, "index.html");
        File.WriteAllText(indexPath, "<html></html>");

        try
        {
            var result = new GoldenPublishResult
            {
                DestinationDirectory = tempDir,
                IndexHtmlPath = indexPath,
            };
            await Assert.That(result.IndexHtmlUri).StartsWith("file:");
            await Assert.That(result.IndexHtmlUri).Contains("index.html");
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }
}
