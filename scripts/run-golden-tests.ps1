# Local parity with CI golden-tests job (Windows + native raylib required).
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
Push-Location $root
try {
    dotnet run pipeline/raylib6/fetch-sources.cs
    dotnet run pipeline/raylib6/run.cs native
    dotnet build Novolis.Raylib.slnx -c Release
    dotnet test tests/Novolis.Raylib.Golden/Novolis.Raylib.Golden.csproj -c Release --filter "Category=Golden" -- --maximum-parallel-tests 1
}
finally {
    Pop-Location
}
