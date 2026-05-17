using System.Reflection;

namespace Novolis.Raylib.Testing.Golden;

/// <summary>Resolves run and story directories for golden QA output (adhoc, solution-run, etc.).</summary>
public interface IGoldenRunBucketLayout
{
    GoldenRenderRunContext Resolve(Assembly testAssembly, string storyId, string outputRoot);
}
