namespace Novolis.Raylib.Pipeline;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        if (args is [] or ["-h"] or ["--help"])
        {
            PrintHelp();
            return args is [] ? 1 : 0;
        }

        var force = args.Contains("--force", StringComparer.Ordinal);
        var filtered = args.Where(a => !string.Equals(a, "--force", StringComparison.Ordinal)).ToArray();
        if (filtered.Length == 0)
        {
            PrintHelp();
            return 1;
        }

        var runner = new PipelineRunner(PipelineStepRegistry.CreateAll());

        try
        {
            return filtered[0].ToLowerInvariant() switch
            {
                "run" when filtered.Length >= 2 => await runner.RunProfileAsync(filtered[1], force),
                "list" => ListSteps(),
                "explain" when filtered.Length >= 2 => Explain(filtered[1]),
                _ => Unknown(filtered[0]),
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }

    private static int ListSteps()
    {
        foreach (var step in PipelineStepRegistry.CreateAll())
            Console.WriteLine($"{step.Id,-24}  {step.Description}");
        Console.WriteLine();
        Console.WriteLine("Profiles: all, maintainer, generate, ci-codegen, agent-verify");
        return 0;
    }

    private static int Explain(string stepId)
    {
        var text = PipelineProfiles.Explain(stepId);
        if (text is null)
        {
            Console.Error.WriteLine($"Unknown step: {stepId}");
            return 1;
        }

        Console.WriteLine(text);
        return 0;
    }

    private static int Unknown(string cmd)
    {
        Console.Error.WriteLine($"Unknown command: {cmd}");
        PrintHelp();
        return 1;
    }

    private static void PrintHelp()
    {
        Console.WriteLine("""
            Novolis.Raylib — linear maintainer pipeline

            Commands:
              run <profile|step_id> [--force]  — run a profile or single step
              list                             — list steps and profiles
              explain <step_id>                — describe a step

            Profiles:
              all / maintainer   step_01 .. step_07
              generate           step_03 + step_06
              ci-codegen         step_04 .. step_07
              agent-verify       ci-codegen + step_08_build

            Step results: codegen/pipeline/raylib6/steps/<step>/result.json + step.log
            """);
    }
}
