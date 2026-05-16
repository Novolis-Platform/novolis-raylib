# Blocks agent Write/StrReplace on committed generated *.g.cs (Bindings interop + Runtime façades).
# Exit 2 = deny. See agentic-tools/registry.json
$ErrorActionPreference = "Stop"
$inputRaw = [Console]::In.ReadToEnd()
if ([string]::IsNullOrWhiteSpace($inputRaw)) {
    Write-Output '{"permission":"allow"}'
    exit 0
}

$payload = $inputRaw | ConvertFrom-Json
$toolName = [string]$payload.tool_name
$filePath = [string]$payload.file_path
if ([string]::IsNullOrWhiteSpace($filePath)) {
    $filePath = [string]$payload.path
}

$isEditTool = $toolName -match '^(Write|StrReplace|edit|EditNotebook)$'
if (-not $isEditTool -or [string]::IsNullOrWhiteSpace($filePath)) {
    Write-Output '{"permission":"allow"}'
    exit 0
}

$normalized = ($filePath -replace '\\', '/').ToLowerInvariant()
$blockedPatterns = @(
    '/src/novolis.raylib.bindings/interop/',
    '/src/novolis.raylib.runtime/'
)

$isGenerated = $false
foreach ($p in $blockedPatterns) {
    if ($normalized.Contains($p) -and $normalized.EndsWith('.g.cs')) {
        $isGenerated = $true
        break
    }
}

if (-not $isGenerated) {
    Write-Output '{"permission":"allow"}'
    exit 0
}

$msg = @"
Direct edits to generated *.g.cs are not allowed in this repo.
Edit pipeline/raylib6/*.manifest.json (or CodeGen hooks), then run:
  dotnet run --project codegen/Novolis.Raylib.CodeGen -- generate
  pwsh ./scripts/agent-verify.ps1
See agentic-tools/workflows/codegen.md
"@

$json = @{
    permission    = "deny"
    user_message  = "Blocked edit to auto-generated binding file."
    agent_message = $msg
} | ConvertTo-Json -Compress

Write-Output $json
exit 2
