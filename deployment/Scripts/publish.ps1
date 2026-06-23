# publish.ps1 — builds a self-contained, single-folder Release publish of
# RMS.WPF, ready to be picked up by the Inno Setup script.
#
# Usage:  .\deployment\Scripts\publish.ps1

$ErrorActionPreference = "Stop"

$root = Resolve-Path "$PSScriptRoot\..\.."
$project = Join-Path $root "src\Desktop\RMS.WPF\RMS.WPF.csproj"

Write-Host "Publishing RMS.WPF (Release, win-x64, self-contained)..." -ForegroundColor Cyan

dotnet publish $project `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=false `
    -p:IncludeNativeLibrariesForSelfExtract=true

Write-Host "Publish complete. Output:" -ForegroundColor Green
Write-Host "  src\Desktop\RMS.WPF\bin\Release\net10.0-windows\win-x64\publish\"
