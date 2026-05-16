$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
Push-Location $root
try {
    dotnet run --project codegen/Novolis.Raylib.CodeGen -- generate
    git diff --exit-code src/Novolis.Raylib.Bindings/ src/Novolis.Raylib.Runtime/
    Write-Host "codegen check: OK"
}
finally {
    Pop-Location
}
