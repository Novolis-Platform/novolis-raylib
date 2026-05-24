using System.Reflection;

namespace Novolis.Raylib.Testing.Golden;

/// <summary>Resolves run and story directories for golden QA output (adhoc, solution-run, etc.).</summary>
public interface IGoldenRunBucketLayout
{
    /// <summary>Resolves output paths for one golden story run.</summary>
    /// <param name="testAssembly">Test assembly for segmentation.</param>
    /// <param name="storyId">Story identifier.</param>
    /// <param name="outputRoot">Absolute output root directory.</param>
    /// <returns>Run and story directory paths.</returns>
    GoldenRenderRunContext Resolve(Assembly testAssembly, string storyId, string outputRoot);
}
