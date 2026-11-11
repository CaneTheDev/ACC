#!/usr/bin/env pwsh
# Publish optimized single-file executable (framework-dependent)

Write-Host "Publishing AccountTimer (Release, Framework-Dependent)..." -ForegroundColor Cyan
Write-Host ""

# Kill any running instances
Write-Host "[0/4] Stopping running instances..." -ForegroundColor Yellow
Get-Process -Name "AccountTimer" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Milliseconds 500

$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()

# Clean first
Write-Host "[1/4] Cleaning..." -ForegroundColor Yellow
dotnet clean -c Release -v quiet

# Publish as single file WITHOUT bundling the framework
Write-Host "[2/4] Publishing optimized build..." -ForegroundColor Yellow
dotnet publish -c Release -r win-x64 `
    --self-contained false `
    -p:PublishSingleFile=true `
    -p:DebugType=none `
    -p:DebugSymbols=false `
    -v minimal

if ($LASTEXITCODE -ne 0) {
    Write-Host "Publish failed!" -ForegroundColor Red
    exit 1
}

$stopwatch.Stop()
$buildTime = [math]::Round($stopwatch.Elapsed.TotalSeconds, 1)

Write-Host ""
Write-Host "[3/4] Build complete in ${buildTime}s" -ForegroundColor Green
Write-Host ""

$exePath = "bin\Release\net8.0-windows\win-x64\publish\AccountTimer.exe"
if (Test-Path $exePath) {
    $fileSize = [math]::Round((Get-Item $exePath).Length / 1MB, 1)
    Write-Host "[4/4] Success!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Executable: $exePath" -ForegroundColor Cyan
    Write-Host "File size: ${fileSize} MB" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Note: Requires .NET 8 Runtime (already on your system)" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Run with: .\bin\Release\net8.0-windows\win-x64\publish\AccountTimer.exe" -ForegroundColor Green
}
