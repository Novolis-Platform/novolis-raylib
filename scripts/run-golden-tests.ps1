# Local parity with CI golden-tests job (Windows + native raylib required).
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
Push-Location $root
try {
    dotnet run --project codegen/Novolis.Raylib.Pipeline -- run step_01_source
    dotnet run --project codegen/Novolis.Raylib.Pipeline -- run step_02_native
    dotnet build Novolis.Raylib.slnx -c Release
    dotnet test tests/Novolis.Raylib.Golden/Novolis.Raylib.Golden.csproj -c Release --filter "Category=Golden" -- --maximum-parallel-tests 1
}
finally {
    Pop-Location
}
