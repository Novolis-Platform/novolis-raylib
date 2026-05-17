// DEPRECATED — use: dotnet run --project codegen/Novolis.Raylib.CodeGen -- generate
#:sdk Microsoft.NET.Sdk
#:property TargetFramework=net10.0
#:property PublishAot=false
#:property PackAsTool=false

Console.Error.WriteLine(
    "This script is obsolete. Regenerate debug hooks with: dotnet run --project codegen/Novolis.Raylib.CodeGen -- generate");
return 1;
