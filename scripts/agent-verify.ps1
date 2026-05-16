# Agent gate: codegen drift check + minimal build of Bindings and Runtime.
# Registry id: agent.verify (see agentic-tools/registry.json)
param(
    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
Push-Location $root
try {
    Write-Host "== agent.verify: codegen drift =="
    & "$PSScriptRoot/raylib-codegen-check.ps1"
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    if ($SkipBuild) {
        Write-Host "agent.verify: OK (build skipped)"
        exit 0
    }

    Write-Host "== agent.verify: build Bindings + Runtime =="
    dotnet build "src/Novolis.Raylib.Bindings/Novolis.Raylib.Bindings.csproj" -c Release
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    dotnet build "src/Novolis.Raylib.Runtime/Novolis.Raylib.Runtime.csproj" -c Release
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    Write-Host "agent.verify: OK"
    exit 0
}
finally {
    Pop-Location
}
