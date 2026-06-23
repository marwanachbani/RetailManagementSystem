# build-installer.ps1 — runs publish.ps1 then compiles the Inno Setup script.
# Requires Inno Setup 6 installed with ISCC.exe on PATH (or update $iscc below).
#
# Usage:  .\deployment\Scripts\build-installer.ps1

$ErrorActionPreference = "Stop"
$root = Resolve-Path "$PSScriptRoot\..\.."

& "$PSScriptRoot\publish.ps1"

$iscc = "ISCC.exe"
$issFile = Join-Path $root "deployment\InnoSetup\RetailManagementSystem.iss"

Write-Host "Compiling installer with Inno Setup..." -ForegroundColor Cyan
& $iscc $issFile

Write-Host "Installer build complete. See deployment\artifacts\" -ForegroundColor Green
