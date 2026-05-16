$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
Push-Location $root
try {
    dotnet run --project codegen/Novolis.Raylib.CodeGen -- enrich-docs
    dotnet run --project codegen/Novolis.Raylib.CodeGen -- verify-docs
    Write-Host "doc-check: OK"
}
finally {
    Pop-Location
}
