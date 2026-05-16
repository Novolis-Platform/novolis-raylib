# After editing a pipeline manifest, remind agent to regenerate and verify.
$ErrorActionPreference = "Stop"
$inputRaw = [Console]::In.ReadToEnd()
if ([string]::IsNullOrWhiteSpace($inputRaw)) { exit 0 }

$payload = $inputRaw | ConvertFrom-Json
$filePath = [string]$payload.file_path
if ([string]::IsNullOrWhiteSpace($filePath)) { $filePath = [string]$payload.path }
if ([string]::IsNullOrWhiteSpace($filePath)) { exit 0 }

$normalized = $filePath -replace '\\', '/'
if ($normalized -notmatch 'pipeline/raylib6/.*\.manifest\.json$') { exit 0 }

$ctx = @{
    additional_context = @"
Manifest edited: $normalized
Required next steps (agentic-tools):
1. dotnet run --project codegen/Novolis.Raylib.CodeGen -- generate
2. pwsh ./scripts/agent-verify.ps1
Commit manifest and regenerated *.g.cs together. Do not hand-edit *.g.cs.
"@
} | ConvertTo-Json -Compress

Write-Output $ctx
exit 0
