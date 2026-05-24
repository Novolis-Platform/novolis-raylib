# Pack shipping Novolis.Raylib.* packages only (no codegen / pipeline).
param(
    [string]$Version,
    [string]$Output = "artifacts/packages"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot

if (-not $Version -and (Test-Path (Join-Path $root ".novolis/version.props"))) {
    $props = Get-Content (Join-Path $root ".novolis/version.props") -Raw
    $major = if ($props -match '<NovolisVersionMajor>(\d+)</') { $Matches[1] } else { '0' }
    $minor = if ($props -match '<NovolisVersionMinor>(\d+)</') { $Matches[1] } else { '0' }
    $patch = if ($props -match '<NovolisVersionPatch>(\d+)</') { $Matches[1] } else { '1' }
    $build = if ($props -match '<NovolisVersionBuild>(\d+)</') { $Matches[1] } else { '1' }
    $Version = "$major.$minor.$patch.$build"
}

$shipping = @(
    "src/Novolis.Raylib.Native/Novolis.Raylib.Native.csproj",
    "src/Novolis.Raylib.Abstractions/Novolis.Raylib.Abstractions.csproj",
    "src/Novolis.Raylib.Bindings/Novolis.Raylib.Bindings.csproj",
    "src/Novolis.Raylib.Runtime/Novolis.Raylib.Runtime.csproj",
    "src/Novolis.Raylib.Capture/Novolis.Raylib.Capture.csproj",
    "src/Novolis.Raylib.Hosting/Novolis.Raylib.Hosting.csproj",
    "src/Novolis.Raylib.Game/Novolis.Raylib.Game.csproj",
    "src/Novolis.Raylib/Novolis.Raylib.csproj",
    "src/Novolis.Raylib.Testing/Novolis.Raylib.Testing.csproj",
    "src/Novolis.Raylib.Raygui/Novolis.Raylib.Raygui.csproj",
    "src/Novolis.Raylib.Raygui.Native/Novolis.Raylib.Raygui.Native.csproj"
)

$versionArgs = @()
if ($Version) {
    $versionArgs = @("-p:Version=$Version", "-p:PackageVersion=$Version")
}

Push-Location $root
New-Item -ItemType Directory -Force -Path $Output | Out-Null
foreach ($p in $shipping) {
    if (-not (Test-Path $p)) { throw "Missing shipping project: $p" }
    dotnet pack $p -c Release -o $Output @versionArgs
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}
Pop-Location

Get-ChildItem (Join-Path $root $Output) -Filter "*.nupkg" | Select-Object -ExpandProperty Name
