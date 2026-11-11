#!/usr/bin/env pwsh
# Quick run script - skips build

Write-Host "Launching AccountTimer..." -ForegroundColor Cyan

# Kill existing instances
Get-Process -Name "AccountTimer" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Milliseconds 300

dotnet run -c Debug -r win-x64 --no-build
