# Regenerates committed golden baseline PNGs and spec hashes (Windows + native raylib required).
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
Push-Location $root
try {
    dotnet run pipeline/raylib6/run.cs all
    dotnet run --project tests/Novolis.Raylib.Golden.Seed/Novolis.Raylib.Golden.Seed.csproj -c Release
    Write-Host "Review tests/Novolis.Raylib.Golden/Goldens/ and commit baseline.png + spec.json changes."
}
finally {
    Pop-Location
}
