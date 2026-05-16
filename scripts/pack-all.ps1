param(
    [string]$Version
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$pkgs = @(
    "src/Novolis.Raylib.Native/Novolis.Raylib.Native.csproj",
    "src/Novolis.Raylib.Abstractions/Novolis.Raylib.Abstractions.csproj",
    "src/Novolis.Raylib.Bindings/Novolis.Raylib.Bindings.csproj",
    "src/Novolis.Raylib.Runtime/Novolis.Raylib.Runtime.csproj",
    "src/Novolis.Raylib.Hosting/Novolis.Raylib.Hosting.csproj",
    "src/Novolis.Raylib.Game/Novolis.Raylib.Game.csproj",
    "src/Novolis.Raylib/Novolis.Raylib.csproj",
    "src/Novolis.Raylib.Testing/Novolis.Raylib.Testing.csproj"
)
Push-Location $root
New-Item -ItemType Directory -Path artifacts -Force | Out-Null
foreach ($p in $pkgs) {
    if ($Version) {
        dotnet pack $p -c Release -o artifacts /p:IsPacking=true "/p:NovolisRaylibVersion=$Version"
    } else {
        dotnet pack $p -c Release -o artifacts /p:IsPacking=true
    }
}
Pop-Location
Get-ChildItem (Join-Path $root artifacts) -Filter Novolis.Raylib*.nupkg | Select-Object Name
