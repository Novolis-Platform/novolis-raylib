$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
Push-Location $root
try {
    dotnet run --project codegen/Novolis.Raylib.Pipeline -- run ci-codegen
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    Write-Host "codegen check: OK"
}
finally {
    Pop-Location
}
