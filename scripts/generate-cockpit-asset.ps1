$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$outDir = Join-Path $root "samples/XFighter/Assets"
$outPath = Join-Path $outDir "cockpit_overlay.png"
New-Item -ItemType Directory -Path $outDir -Force | Out-Null

Add-Type -AssemblyName System.Drawing
$w = 1920
$h = 1080
$bmp = New-Object System.Drawing.Bitmap $w, $h
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias

$g.Clear([System.Drawing.Color]::FromArgb(0, 0, 0, 0))
$panel = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(235, 18, 22, 32))
$bevel = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(255, 55, 65, 85))
$consoleBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(200, 28, 34, 48))
$viewL = [int]($w * 0.14)
$viewR = [int]($w * 0.86)
$viewT = [int]($h * 0.12)
$viewB = [int]($h * 0.88)
$margin = 42

$g.FillRectangle($panel, 0, 0, $w, $margin)
$g.FillRectangle($panel, 0, $h - $margin, $w, $margin)
$g.FillRectangle($panel, 0, 0, $viewL, $h)
$g.FillRectangle($panel, 0, $viewR, $w - $viewR, $h)
$g.FillRectangle($bevel, $viewL, $margin, 8, $viewB - $margin)
$g.FillRectangle($bevel, $viewR - 8, $margin, 8, $viewB - $margin)
$g.FillRectangle($bevel, $viewL, $viewT - 8, $viewR - $viewL, 8)
$g.FillRectangle($bevel, $viewL, $viewB, $viewR - $viewL, 8)

for ($i = 0; $i -lt 10; $i++) {
    $y = $margin + 20 + $i * 46
    $g.FillRectangle($consoleBrush, $viewL + 28, $y, 140, 32)
    $g.FillRectangle($consoleBrush, $viewR - 168, $y, 140, 32)
}

$panel.Dispose()
$bevel.Dispose()
$consoleBrush.Dispose()
$g.Dispose()
$bmp.Save($outPath, [System.Drawing.Imaging.ImageFormat]::Png)
$bmp.Dispose()
Write-Host "Wrote $outPath"
