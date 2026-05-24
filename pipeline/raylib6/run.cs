// Obsolete forwarder — use: dotnet run --project codegen/Novolis.Raylib.Pipeline -- run <profile>
#:sdk Microsoft.NET.Sdk
#:property TargetFramework=net10.0
#:property PublishAot=false
#:property PackAsTool=false
#:property ImplicitUsings=enable
#:property Nullable=enable
#:property ManagePackageVersionsCentrally=false

var repoRoot = FindRepoRoot();
var pipeline = Path.Combine(repoRoot, "codegen", "Novolis.Raylib.Pipeline", "Novolis.Raylib.Pipeline.csproj");

if (args is [] or ["-h"] or ["--help"])
{
	Console.WriteLine("""
		Obsolete — use codegen/Novolis.Raylib.Pipeline instead.

		  dotnet run --project codegen/Novolis.Raylib.Pipeline -- run maintainer
		  dotnet run --project codegen/Novolis.Raylib.Pipeline -- run step_01_source
		  dotnet run --project codegen/Novolis.Raylib.Pipeline -- list
		""");
	return args is [] ? 1 : 0;
}

var profile = args[0].ToLowerInvariant() switch
{
	"fetch" => "step_01_source",
	"native" => "step_02_native",
	"generate" => "generate",
	"all" => "maintainer",
	_ => args[0],
};

Console.Error.WriteLine($"run.cs is obsolete. Forwarding to Pipeline profile '{profile}'.");
return Run("dotnet", $"run --project \"{pipeline}\" -- run {profile}", repoRoot);

static int Run(string file, string arguments, string workingDirectory)
{
	var psi = new System.Diagnostics.ProcessStartInfo
	{
		FileName = file,
		Arguments = arguments,
		WorkingDirectory = workingDirectory,
		UseShellExecute = false,
	};
	using var p = System.Diagnostics.Process.Start(psi) ?? throw new InvalidOperationException($"Failed to start {file}");
	p.WaitForExit();
	return p.ExitCode;
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
