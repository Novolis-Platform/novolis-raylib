# Opens the newest golden QA index.html under temp/test-renders.
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$rendersRoot = Join-Path $root "temp\test-renders\adhoc-runs"
if (-not (Test-Path $rendersRoot)) {
    Write-Error "No golden runs found at $rendersRoot. Run golden tests first."
}

$latest = Get-ChildItem -Path $rendersRoot -Recurse -Filter "index.html" -File |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1

if (-not $latest) {
    Write-Error "No index.html under $rendersRoot"
}

Write-Host "Opening $($latest.FullName)"
Start-Process $latest.FullName
