// Orchestrator: fetch vendor → optional native shim build (CMake) → optional ClangSharp (see --help).
// Usage (repo root): dotnet run tools/raylib6-pipeline/run.cs [fetch|native|generate|all]
#:sdk Microsoft.NET.Sdk
#:property TargetFramework=net10.0
#:property PublishAot=false
#:property PackAsTool=false
#:property ImplicitUsings=enable
#:property Nullable=enable
#:property ManagePackageVersionsCentrally=false

using System.Diagnostics;
using System.Linq;

var repoRoot = FindRepoRoot();
if (args is [] or ["-h"] or ["--help"])
{
	Console.WriteLine("""
		Star Conflicts Revolt — raylib 6 pipeline

		Commands:
		  fetch    — dotnet run tools/raylib6-pipeline/fetch-sources.cs
		  native   — cmake configure/build tools/native/raylib6-with-raygui (Windows + MSVC)
		  generate — Roslyn codegen (verify → interop → raygui → debug hooks → façades + hooks)
		  all      — fetch, then native, then generate (same codegen chain as generate)

		Prerequisites (native): CMake + VS C toolchain; run fetch first so tools/vendor/raylib-6 contains raylib.dll.
		""");
	return args is [] ? 1 : 0;
}

var cmd = args[0].ToLowerInvariant();
try
{
	switch (cmd)
	{
		case "fetch":
			return RunDotnetFile(Path.Combine(repoRoot, "pipeline", "raylib6", "fetch-sources.cs"), []);
		case "native":
			return RunNative(repoRoot);
		case "generate":
			return RunGenerateChain(repoRoot);
		case "all":
		{
			var a = RunDotnetFile(Path.Combine(repoRoot, "pipeline", "raylib6", "fetch-sources.cs"), []);
			if (a != 0)
				return a;
			a = RunNative(repoRoot);
			if (a != 0)
				return a;
			return RunGenerateChain(repoRoot);
		}
		default:
			Console.Error.WriteLine($"Unknown command: {args[0]}");
			return 1;
	}
}
	catch (Exception ex)
{
	Console.Error.WriteLine(ex.Message);
	return 1;
}

static int RunGenerateChain(string repoRoot)
{
	var codegenProject = Path.Combine(repoRoot, "codegen", "Novolis.Raylib.CodeGen", "Novolis.Raylib.CodeGen.csproj");
	return Run("dotnet", $"run --project \"{codegenProject}\" -- generate", repoRoot);
}

static int RunNative(string repoRoot)
{
	var nativeDir = Path.Combine(repoRoot, "native", "raylib6-with-raygui");
	var defPath = Path.Combine(nativeDir, "generated", "raylib.win-x64.def");
	if (OperatingSystem.IsWindows())
	{
		var raylibDll = Path.Combine(repoRoot, "vendor", "raylib-6", "prebuilt", "win-x64", "raylib.dll");
		if (File.Exists(raylibDll) && !File.Exists(defPath))
		{
			var gen = Path.Combine(repoRoot, "pipeline", "raylib6", "generate-raylib-windows-def.cs");
			var a = RunDotnetFile(gen, [raylibDll, defPath]);
			if (a != 0)
				return a;
		}
	}

	var buildDir = Path.Combine(nativeDir, "build");
	Directory.CreateDirectory(buildDir);
	var configureArgs = $"-S \"{nativeDir}\" -B \"{buildDir}\"";
	if (Run("cmake", configureArgs, repoRoot) != 0)
	{
		// Stale CMake cache (e.g. sources moved from repo-root native/ to tools/native/) — wipe once and retry.
		try
		{
			Directory.Delete(buildDir, recursive: true);
		}
		catch
		{
			// ignore; second cmake will surface the real error
		}

		Directory.CreateDirectory(buildDir);
		if (Run("cmake", configureArgs, repoRoot) != 0)
			return 1;
	}

	return Run("cmake", $"--build \"{buildDir}\" --config Release", repoRoot);
}

static int Run(string file, string arguments, string workingDirectory)
{
	var psi = new ProcessStartInfo
	{
		FileName = file,
		Arguments = arguments,
		WorkingDirectory = workingDirectory,
		UseShellExecute = false,
	};
	using var p = Process.Start(psi) ?? throw new InvalidOperationException($"Failed to start {file}");
	p.WaitForExit();
	return p.ExitCode;
}

static int RunDotnetFile(string path, string[] extraArgs)
{
	var parts = new List<string> { "run", $"\"{path}\"" };
	if (extraArgs.Length > 0)
	{
		parts.Add("--");
		parts.AddRange(extraArgs.Select(a => $"\"{a}\""));
	}

	var args = string.Join(' ', parts);
	return Run("dotnet", args, FindRepoRoot());
}

static string FindRepoRoot()
{
	var dir = new DirectoryInfo(AppContext.BaseDirectory);
	while (dir is not null)
	{
		if (File.Exists(Path.Combine(dir.FullName, "Directory.Packages.props")))
			return dir.FullName;
		dir = dir.Parent;
	}

	return Directory.GetCurrentDirectory();
}
