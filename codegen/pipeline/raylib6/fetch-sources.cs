// Obsolete forwarder — use: dotnet run --project codegen/Novolis.Raylib.Pipeline -- run step_01_source
#:sdk Microsoft.NET.Sdk
#:property TargetFramework=net10.0
#:property PublishAot=false
#:property PackAsTool=false
#:property ImplicitUsings=enable
#:property Nullable=enable
#:property ManagePackageVersionsCentrally=false

Console.Error.WriteLine("fetch-sources.cs is obsolete. Use: dotnet run --project codegen/Novolis.Raylib.Pipeline -- run step_01_source");
var repoRoot = FindRepoRoot();
var pipeline = Path.Combine(repoRoot, "codegen", "Novolis.Raylib.Pipeline", "Novolis.Raylib.Pipeline.csproj");
return Run("dotnet", $"run --project \"{pipeline}\" -- run step_01_source", repoRoot);

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
