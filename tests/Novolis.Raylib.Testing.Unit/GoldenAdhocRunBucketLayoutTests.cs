using Novolis.Raylib.Abstractions;
using Novolis.Raylib.Interact;
using Novolis.Raylib.Testing;
using Novolis.Raylib.Testing.Golden;

namespace Novolis.Raylib.Testing.Unit;

[NotInParallel("golden-adhoc-bucket")]
public sealed class GoldenAdhocRunBucketLayoutTests
{
    [Test]
    public async Task Resolve_creates_story_directory_under_shared_run()
    {
        GoldenAdhocRunBucketLayout.ResetSharedRun();
        var tempRoot = Path.Combine(Path.GetTempPath(), "novolis-testing-unit", Guid.NewGuid().ToString("N"));
        try
        {
            var ctx = GoldenAdhocRunBucketLayout.Instance.Resolve(typeof(GoldenAdhocRunBucketLayoutTests).Assembly, "story-a", tempRoot);
            await Assert.That(Directory.Exists(ctx.StoryDirectory)).IsTrue();
            await Assert.That(ctx.StoryId).IsEqualTo("story-a");
            await Assert.That(GoldenAdhocRunBucketLayout.SharedRunFolder).IsNotNull();
            var ctx2 = GoldenAdhocRunBucketLayout.Instance.Resolve(typeof(GoldenAdhocRunBucketLayoutTests).Assembly, "story-b", tempRoot);
            await Assert.That(ctx2.RunFolder).IsEqualTo(ctx.RunFolder);
        }
        finally
        {
            GoldenAdhocRunBucketLayout.ResetSharedRun();
            if (Directory.Exists(tempRoot))
                Directory.Delete(tempRoot, recursive: true);
        }
    }
}
