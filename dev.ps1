#!/usr/bin/env pwsh
# Development script for AccountTimer WPF app

param([switch]$Clean)

$ErrorActionPreference = "Stop"
Write-Host "AccountTimer - Development Build" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan
Write-Host ""

$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()

if ($Clean) {
    Write-Host "[1/3] Cleaning..." -ForegroundColor Yellow
    dotnet clean -v quiet
}

Write-Host "[1/3] Restoring packages..." -ForegroundColor Yellow
dotnet restore -r win-x64 -v quiet

Write-Host "[2/3] Building (x64, Debug)..." -ForegroundColor Yellow
dotnet build -c Debug -r win-x64 --no-restore -v quiet

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

$stopwatch.Stop()
$buildTime = [math]::Round($stopwatch.Elapsed.TotalSeconds, 1)
Write-Host "Build complete in ${buildTime}s" -ForegroundColor Green
Write-Host ""

Write-Host "[3/3] Launching application..." -ForegroundColor Yellow
Write-Host "=================================" -ForegroundColor Cyan
Write-Host ""

# Kill existing instances
Get-Process -Name "AccountTimer" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Milliseconds 300

# Run and capture all output
$output = dotnet run -c Debug -r win-x64 --no-build 2>&1 | Out-String
Write-Host $output

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "Application exited with error code: $LASTEXITCODE" -ForegroundColor Red
    Write-Host "Output above may contain error details." -ForegroundColor Yellow
}
