namespace Novolis.Raylib.Pipeline;

internal static class PipelineStepRegistry
{
    public static IReadOnlyList<IPipelineStep> CreateAll() =>
    [
        new Steps.SourceStep(),
        new Steps.NativeStep(),
        new Steps.VerifyManifestStep(),
        new Steps.EnrichDocsStep(),
        new Steps.VerifyDocsStep(),
        new Steps.CodegenStep(),
        new Steps.DriftStep(),
        new Steps.BuildStep(),
    ];
}
