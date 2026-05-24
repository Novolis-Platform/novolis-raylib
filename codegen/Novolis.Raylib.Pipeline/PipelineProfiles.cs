namespace Novolis.Raylib.Pipeline;

internal static class PipelineProfiles
{
    public static IReadOnlyList<string> AllStepIds { get; } =
    [
        "step_01_source",
        "step_02_native",
        "step_03_verify_manifest",
        "step_04_enrich_docs",
        "step_05_verify_docs",
        "step_06_codegen",
        "step_07_drift",
        "step_08_build",
    ];

    public static IReadOnlyList<string> Resolve(string profile)
    {
        return profile.ToLowerInvariant() switch
        {
            "all" or "maintainer" =>
            [
                "step_01_source",
                "step_02_native",
                "step_03_verify_manifest",
                "step_04_enrich_docs",
                "step_05_verify_docs",
                "step_06_codegen",
                "step_07_drift",
            ],
            "generate" =>
            [
                "step_03_verify_manifest",
                "step_06_codegen",
            ],
            "ci-codegen" =>
            [
                "step_04_enrich_docs",
                "step_05_verify_docs",
                "step_06_codegen",
                "step_07_drift",
            ],
            "agent-verify" =>
            [
                "step_04_enrich_docs",
                "step_05_verify_docs",
                "step_06_codegen",
                "step_07_drift",
                "step_08_build",
            ],
            _ when AllStepIds.Contains(profile, StringComparer.Ordinal) => [profile],
            _ => throw new InvalidOperationException(
                $"Unknown profile '{profile}'. Use: all, maintainer, generate, ci-codegen, agent-verify, or a step id."),
        };
    }

    public static string? Explain(string stepId) =>
        stepId switch
        {
            "step_01_source" => "Fetch raylib prebuilts, raygui header, and raylib-cimgui into step_01_source/artifacts/.",
            "step_02_native" => "CMake build trace/ImGui/raygui shims; copy outputs to step_02_native/artifacts/.",
            "step_03_verify_manifest" => "Verify manifest imports exist in fetched raylib.h.",
            "step_04_enrich_docs" => "Fill missing façade summaries from headers (writes manifests).",
            "step_05_verify_docs" => "Fail if façade docs are missing.",
            "step_06_codegen" => "Emit committed *.g.cs from manifests + hooks.",
            "step_07_drift" => "git diff on pipeline manifests and generated C#.",
            "step_08_build" => "Release build Bindings + Runtime.",
            _ => null,
        };
}
