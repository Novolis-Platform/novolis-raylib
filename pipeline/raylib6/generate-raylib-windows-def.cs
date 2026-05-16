// Maintainer: emit a .def for raylib.dll so MSVC lib.exe can build an import library for the raygui shim.
// Usage (repo root): dotnet run tools/raylib6-pipeline/generate-raylib-windows-def.cs -- <path-to-raylib.dll> <output.def>
#:sdk Microsoft.NET.Sdk
#:property TargetFramework=net10.0
#:property PublishAot=false
#:property PackAsTool=false
#:property ImplicitUsings=enable
#:property Nullable=enable
#:property ManagePackageVersionsCentrally=false
#:package PeNet@4.1.2

using System.Text;
using PeNet;

if (args is ["-h"] or ["--help"])
{
	Console.WriteLine("""
		Usage:
		  dotnet run tools/raylib6-pipeline/generate-raylib-windows-def.cs -- <path-to-raylib.dll> <output.def>

		Emits a module-definition file listing exports from the given raylib.dll.
		Used by tools/native/raylib6-with-raygui (Windows) to build raylib_import.lib via MSVC lib.exe.
		""");
	return 0;
}

if (args.Length < 2)
{
	Console.Error.WriteLine("Usage: dotnet run tools/raylib6-pipeline/generate-raylib-windows-def.cs -- <path-to-raylib.dll> <output.def>");
	return 1;
}

var dllPath = Path.GetFullPath(args[0]);
var outPath = Path.GetFullPath(args[1]);
if (!File.Exists(dllPath))
{
	Console.Error.WriteLine($"DLL not found: {dllPath}");
	return 2;
}

var pe = new PeFile(dllPath);
var exports = pe.ExportedFunctions?
		.Select(static e => e.Name)
		.Where(static s => !string.IsNullOrEmpty(s))
		.Distinct(StringComparer.Ordinal)
		.Order(StringComparer.Ordinal)
		.ToArray()
	?? Array.Empty<string>();

var sb = new StringBuilder();
sb.AppendLine("LIBRARY raylib");
sb.AppendLine("EXPORTS");
foreach (var e in exports)
	sb.AppendLine(e);

Directory.CreateDirectory(Path.GetDirectoryName(outPath)!);
File.WriteAllText(outPath, sb.ToString(), Encoding.ASCII);
Console.WriteLine($"Wrote {exports.Length} exports to {outPath}");
return 0;
