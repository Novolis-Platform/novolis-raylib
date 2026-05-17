using Novolis.Raylib.Testing;
using Novolis.Raylib.Testing.Golden;

namespace Novolis.Raylib.Golden;

public sealed class GoldenCatalogConsistencyTests
{
    [Test]
    public async Task Committed_golden_specs_match_folder_names_and_baseline_hashes()
    {
        var assembly = typeof(GoldenCatalogConsistencyTests).Assembly;
        var root = GoldenCatalog.GetGoldensRoot(assembly);

        foreach (var specPath in Directory.EnumerateFiles(root, "spec.json", SearchOption.AllDirectories))
        {
            var storyDir = Path.GetDirectoryName(specPath)!;
            var folderId = Path.GetFileName(storyDir);
            var spec = GoldenStorySpec.LoadFromFile(specPath);

            await Assert.That(spec.StoryId).IsEqualTo(folderId);

            var baselinePath = Path.Combine(storyDir, spec.BaselinePngFileName);
            await Assert.That(File.Exists(baselinePath)).IsTrue();

            var png = await File.ReadAllBytesAsync(baselinePath);
            var hash = FramebufferAssert.Sha256Hex(png);
            await Assert.That(spec.BaselineSha256).IsEqualTo(hash);
        }
    }
}
