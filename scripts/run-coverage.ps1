#Requires -Version 7.0
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $repoRoot

$coverageDir = Join-Path $repoRoot 'artifacts/coverage'
if (Test-Path $coverageDir) {
    Remove-Item -Recurse -Force $coverageDir
}
New-Item -ItemType Directory -Path $coverageDir -Force | Out-Null

$unitProjects = @(
    'tests/Novolis.Raylib.CodeGen.Unit/Novolis.Raylib.CodeGen.Unit.csproj',
    'tests/Novolis.Raylib.Pipeline.Unit/Novolis.Raylib.Pipeline.Unit.csproj',
    'tests/Novolis.Raylib.Testing.Unit/Novolis.Raylib.Testing.Unit.csproj',
    'tests/Novolis.Raylib.Capture.Unit/Novolis.Raylib.Capture.Unit.csproj',
    'tests/Novolis.Raylib.Runtime.Unit/Novolis.Raylib.Runtime.Unit.csproj',
    'tests/Novolis.Raylib.Hosting.Unit/Novolis.Raylib.Hosting.Unit.csproj',
    'tests/Novolis.Raylib.Game.Unit/Novolis.Raylib.Game.Unit.csproj',
    'tests/Novolis.Raylib.Testing.Integration/Novolis.Raylib.Testing.Integration.csproj'
)

function Invoke-CoverageTest {
    param([string]$Project)
    Write-Host "  $Project"
    dotnet test `
        --project $Project `
        -c Release `
        --coverage `
        --coverage-output-format cobertura `
        --results-directory $coverageDir
}

Write-Host 'Running unit tests with coverage...'
foreach ($project in $unitProjects) {
    Invoke-CoverageTest -Project $project
}

if ($env:NOVOLIS_RAYLIB_NATIVE_TESTS -eq '1') {
    Write-Host 'Running golden tests with coverage...'
    Invoke-CoverageTest -Project 'tests/Novolis.Raylib.Golden/Novolis.Raylib.Golden.csproj'
}

$coberturaFiles = @(Get-ChildItem -Path $coverageDir -Recurse -Filter '*.cobertura.xml' -ErrorAction SilentlyContinue)
if ($coberturaFiles.Count -eq 0) {
    Write-Error 'No cobertura files found under artifacts/coverage/'
}

$mergedCobertura = Join-Path $coverageDir 'merged.cobertura.xml'
if ($coberturaFiles.Count -eq 1) {
    Copy-Item $coberturaFiles[0].FullName $mergedCobertura
}
else {
    $reportGen = Get-Command reportgenerator -ErrorAction SilentlyContinue
    if (-not $reportGen) {
        Write-Host 'Installing reportgenerator global tool...'
        dotnet tool install -g dotnet-reportgenerator-globaltool | Out-Null
    }
    $inputs = ($coberturaFiles | ForEach-Object { $_.FullName }) -join ';'
    reportgenerator `
        -reports:$inputs `
        -targetdir:$coverageDir `
        -reporttypes:'Cobertura;Html;JsonSummary' `
        -filefilters:'-*.g.cs' `
        -assemblyfilters:'+*;-*.Tests;-*.Unit;-*Golden*;-*Integration;-*Seed;-XFighter;-Novolis.Raylib'
    if (Test-Path (Join-Path $coverageDir 'Cobertura.xml')) {
        Copy-Item (Join-Path $coverageDir 'Cobertura.xml') $mergedCobertura
    }
    else {
        Copy-Item $coberturaFiles[0].FullName $mergedCobertura
    }
}

$threshold = [double]($env:COVERAGE_THRESHOLD ?? '0')
$failOnThreshold = $threshold -gt 0
& (Join-Path $repoRoot 'scripts/check-coverage-threshold.ps1') `
    -CoberturaPath $mergedCobertura `
    -SummaryJsonPath (Join-Path $repoRoot 'coverage/baseline-summary.json') `
    -ThresholdPercent $threshold `
    -FailOnThreshold:$failOnThreshold

Write-Host "Coverage report: $coverageDir/index.html"
