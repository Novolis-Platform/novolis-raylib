// Placeholder for optional ClangSharp full emit of raylib.h (not used for shipping interop).
// Shipping raylib P/Invoke: tools/raylib6-pipeline/raylib-exports.manifest.json → generate-raylib-interop.cs → Raylib6Native.g.cs
// Shipping raygui: tools/raylib6-pipeline/raygui-exports.manifest.json → generate-raygui-interop.cs (invoked from run.cs generate).
// Usage: dotnet run tools/raylib6-pipeline/generate-bindings.cs
#:sdk Microsoft.NET.Sdk
#:property TargetFramework=net10.0
#:property PublishAot=false
#:property PackAsTool=false
#:property ImplicitUsings=enable
#:property Nullable=enable
#:property ManagePackageVersionsCentrally=false

if (args is ["-h"] or ["--help"])
{
	Console.WriteLine("""
		generate-bindings: reserved for optional ClangSharp full emit of raylib.h.

		Shipping interop today:
		  • raylib — manifest + dotnet run tools/raylib6-pipeline/generate-raylib-interop.cs → Raylib6Native.g.cs
		  • raygui — manifest + dotnet run tools/raylib6-pipeline/generate-raygui-interop.cs
		  • debug hooks — raylib-debug.manifest.json + generate-raylib-debug-hooks.cs

		Set RAYLIB6_RUN_CLANGSHARP=1 to attempt ClangSharp (requires dotnet tool restore in tools/raylib6-pipeline; wire PInvokeGenerator locally).
		""");
	return 0;
}

if (!string.Equals(Environment.GetEnvironmentVariable("RAYLIB6_RUN_CLANGSHARP"), "1", StringComparison.Ordinal))
{
	Console.WriteLine("generate-bindings: skipped (manifest-driven interop). Set RAYLIB6_RUN_CLANGSHARP=1 to run ClangSharp pipeline.");
	return 0;
}

Console.WriteLine("generate-bindings: RAYLIB6_RUN_CLANGSHARP=1 set — integrate ClangSharpPInvokeGenerator locally (no committed rsp yet).");
return 0;
