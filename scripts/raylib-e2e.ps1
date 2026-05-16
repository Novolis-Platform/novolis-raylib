$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$env:NOVOLIS_RAYLIB_OFFSCREEN_TESTS = "1"
$env:NOVOLIS_RAYLIB_NATIVE_TESTS = "1"
Push-Location $root
try {
    dotnet run pipeline/raylib6/run.cs all
    dotnet test tests/Novolis.Raylib.CodeGen.Unit -- --maximum-parallel-tests 1
}
finally {
    Remove-Item Env:NOVOLIS_RAYLIB_OFFSCREEN_TESTS -ErrorAction SilentlyContinue
    Remove-Item Env:NOVOLIS_RAYLIB_NATIVE_TESTS -ErrorAction SilentlyContinue
    Pop-Location
}
