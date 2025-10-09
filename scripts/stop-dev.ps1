#!/usr/bin/env pwsh
# Script to stop Fluxora services

param(
    [switch]$RemoveVolumes
)

Write-Host "🛑 Stopping Fluxora services..." -ForegroundColor Cyan
Write-Host ""

if ($RemoveVolumes) {
    Write-Host "⚠️  Volumes will also be removed (data loss)" -ForegroundColor Yellow
    $confirm = Read-Host "Are you sure? (Y/N)"
    if ($confirm -ne "Y" -and $confirm -ne "y") {
        Write-Host "❌ Operation cancelled" -ForegroundColor Red
        exit 0
    }
    docker-compose down -v
    Write-Host "✅ Services stopped and volumes removed" -ForegroundColor Green
}
else {
    docker-compose down
    Write-Host "✅ Services stopped (data preserved)" -ForegroundColor Green
}

Write-Host ""
Write-Host "💡 To restart services: .\scripts\init-dev.ps1" -ForegroundColor Cyan
Write-Host "💡 To remove volumes: .\scripts\stop-dev.ps1 -RemoveVolumes" -ForegroundColor Cyan
Write-Host ""
