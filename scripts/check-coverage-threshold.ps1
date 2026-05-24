#Requires -Version 7.0
param(
    [Parameter(Mandatory = $true)]
    [string]$CoberturaPath,

    [string]$SummaryJsonPath = '',

    [double]$ThresholdPercent = 70,

    [switch]$FailOnThreshold
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not (Test-Path $CoberturaPath)) {
    Write-Error "Cobertura file not found: $CoberturaPath"
}

[xml]$doc = Get-Content -Raw $CoberturaPath
$coverage = $doc.DocumentElement
$lineRate = [double]$coverage.'line-rate'
$linesValid = [int]$coverage.'lines-valid'
$linesCovered = [int]$coverage.'lines-covered'
$percent = [math]::Round($lineRate * 100, 2)

$assemblyStats = @()
$packages = @($coverage.packages.package)
foreach ($pkg in $packages) {
    $assemblyStats += [ordered]@{
        name = [string]$pkg.name
        lineRate = [math]::Round([double]$pkg.'line-rate' * 100, 2)
    }
}

$summary = [ordered]@{
    generatedUtc = (Get-Date).ToUniversalTime().ToString('o')
    lineRatePercent = $percent
    linesValid = $linesValid
    linesCovered = $linesCovered
    thresholdPercent = $ThresholdPercent
    failOnThreshold = [bool]$FailOnThreshold
    exclusions = @('**/*.g.cs', '**/samples/**')
    ratchetMilestones = @(70, 85, 95)
    assemblies = $assemblyStats
}

$outJson = if ($SummaryJsonPath) { $SummaryJsonPath } else { Join-Path (Split-Path $CoberturaPath) 'coverage-summary.json' }
$summaryDir = Split-Path $outJson -Parent
if ($summaryDir -and -not (Test-Path $summaryDir)) {
    New-Item -ItemType Directory -Path $summaryDir -Force | Out-Null
}
$summary | ConvertTo-Json -Depth 6 | Set-Content -Encoding utf8 $outJson

Write-Host "Line coverage (hand-written): $percent% ($linesCovered / $linesValid lines)"
foreach ($asm in ($assemblyStats | Sort-Object { $_.lineRate })) {
    Write-Host ("  {0}: {1}%" -f $asm.name, $asm.lineRate)
}

if ($FailOnThreshold -and $percent -lt $ThresholdPercent) {
    Write-Error "Coverage $percent% is below threshold $ThresholdPercent%"
}
