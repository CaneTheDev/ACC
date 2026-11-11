#!/usr/bin/env pwsh
# Fast build only

Write-Host "Building AccountTimer..." -ForegroundColor Cyan
$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()

dotnet build -c Debug -r win-x64 --no-restore -v minimal

$stopwatch.Stop()
$seconds = [math]::Round($stopwatch.Elapsed.TotalSeconds, 1)

if ($LASTEXITCODE -eq 0) {
    Write-Host "Build successful in ${seconds}s" -ForegroundColor Green
} else {
    Write-Host "Build failed" -ForegroundColor Red
    exit 1
}
