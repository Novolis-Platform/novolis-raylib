$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
Push-Location $root
try {
    dotnet run --project codegen/Novolis.Raylib.CodeGen -- enrich-docs --write
    dotnet run --project codegen/Novolis.Raylib.CodeGen -- verify-docs
    dotnet run --project codegen/Novolis.Raylib.CodeGen -- generate
    git diff --exit-code pipeline/raylib6/ src/Novolis.Raylib.Bindings/ src/Novolis.Raylib.Runtime/
    Write-Host "codegen check: OK"
}
finally {
    Pop-Location
}
