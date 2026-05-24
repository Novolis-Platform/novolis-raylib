# Agent gate: codegen drift check + minimal build of Bindings and Runtime.
# Registry id: agent.verify (see agentic-tools/registry.json)
param(
    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
Push-Location $root
try {
    Write-Host "== agent.verify: pipeline =="
    if ($SkipBuild) {
        dotnet run --project codegen/Novolis.Raylib.Pipeline -- run ci-codegen
    } else {
        dotnet run --project codegen/Novolis.Raylib.Pipeline -- run agent-verify
    }
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    Write-Host "agent.verify: OK"
    exit 0
}
finally {
    Pop-Location
}
