namespace Novolis.Raylib.CodeGen;

internal static class Program
{
    public static int Main(string[] args)
    {
        if (args is [] or ["-h"] or ["--help"])
        {
            PrintHelp();
            return args is [] ? 1 : 0;
        }

        var repoRoot = RepoPaths.FindRepoRoot();
        var cmd = args[0].ToLowerInvariant();

        try
        {
            return cmd switch
            {
                "generate" => new RaylibCodegenPipeline(repoRoot).GenerateAll(),
                "verify" => RaylibManifestVerifier.Verify(repoRoot),
                "verify-docs" => FacadeDocVerifier.Verify(repoRoot),
                "enrich-docs" => FacadeDocEnricher.Enrich(repoRoot, write: args.Contains("--write")),
                "suggest-raylib" => RaylibManifestSuggester.Suggest(repoRoot),
                "hooks" => RunHooksCommand(args.Skip(1).ToArray()),
                _ => Unknown(cmd),
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }

    private static int RunHooksCommand(string[] args)
    {
        if (args is [] or ["list"])
        {
            foreach (var hook in HookDiscovery.DiscoverAll())
                Console.WriteLine($"{hook.Order,4}  {hook.Phase,-8}  {hook.GetType().FullName}");
            return 0;
        }

        Console.Error.WriteLine("Unknown hooks subcommand. Use: hooks list");
        return 1;
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
            Novolis.Raylib — raylib 6 Roslyn codegen + pipeline

            Commands:
              generate       — emit interop + façades (same as: pipeline run generate)
              verify         — fail if manifest symbols missing from raylib.h
              verify-docs    — fail if façade/Hud/Gui types or methods lack resolvable summaries
              enrich-docs    — fill manifest typeSummary/summary from raylib headers (use --write)
              suggest-raylib — list RLAPI names in raylib.h not present in manifest
              hooks list     — show registered IRaylibCodegenHook implementations

            Maintainer pipeline: dotnet run --project codegen/Novolis.Raylib.Pipeline -- run maintainer
            """);
    }
}
