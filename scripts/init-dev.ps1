#!/usr/bin/env pwsh
# Fluxora development environment initialization script

Write-Host "🚀 Initializing Fluxora environment..." -ForegroundColor Cyan
Write-Host ""

# Check if Docker is installed
Write-Host "📦 Checking Docker..." -ForegroundColor Yellow
if (Get-Command docker -ErrorAction SilentlyContinue) {
    Write-Host "✅ Docker is installed" -ForegroundColor Green
}
else {
    Write-Host "❌ Docker is not installed. Please install Docker Desktop." -ForegroundColor Red
    exit 1
}

# Check if Docker Compose is available
if (Get-Command docker-compose -ErrorAction SilentlyContinue) {
    Write-Host "✅ Docker Compose is available" -ForegroundColor Green
}
else {
    Write-Host "❌ Docker Compose is not available." -ForegroundColor Red
    exit 1
}

# Start Docker services
Write-Host ""
Write-Host "🐳 Starting Docker services (PostgreSQL & Keycloak)..." -ForegroundColor Yellow
docker-compose up -d

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Docker services started successfully" -ForegroundColor Green
}
else {
    Write-Host "❌ Error starting Docker services" -ForegroundColor Red
    exit 1
}

# Wait for services to be ready
Write-Host ""
Write-Host "⏳ Waiting for services to start completely..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

# Check PostgreSQL
Write-Host "🔍 Checking PostgreSQL..." -ForegroundColor Yellow
$maxRetries = 10
$retryCount = 0
$pgReady = $false

while (-not $pgReady -and $retryCount -lt $maxRetries) {
    try {
        _ = docker exec fluxora-postgres pg_isready -U postgres 2>&1
        if ($LASTEXITCODE -eq 0) {
            $pgReady = $true
            Write-Host "✅ PostgreSQL is ready" -ForegroundColor Green
        }
    }
    catch {
        $retryCount++
        Write-Host "⏳ PostgreSQL is not ready yet... ($retryCount/$maxRetries)" -ForegroundColor Yellow
        Start-Sleep -Seconds 5
    }
}

if (-not $pgReady) {
    Write-Host "❌ PostgreSQL did not start correctly" -ForegroundColor Red
    exit 1
}

# Check Keycloak
Write-Host "🔍 Checking Keycloak..." -ForegroundColor Yellow
Write-Host "⏳ Keycloak may take up to 2 minutes to start..." -ForegroundColor Yellow
Start-Sleep -Seconds 30

$keycloakReady = $false
$retryCount = 0
$maxRetries = 12

while (-not $keycloakReady -and $retryCount -lt $maxRetries) {
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:8080/health/ready" -Method Get -TimeoutSec 5 -ErrorAction SilentlyContinue
        if ($response.StatusCode -eq 200) {
            $keycloakReady = $true
            Write-Host "✅ Keycloak is ready" -ForegroundColor Green
        }
    }
    catch {
        $retryCount++
        Write-Host "⏳ Keycloak is not ready yet... ($retryCount/$maxRetries)" -ForegroundColor Yellow
        Start-Sleep -Seconds 10
    }
}

if (-not $keycloakReady) {
    Write-Host "⚠️  Keycloak is taking longer than expected, but continuing..." -ForegroundColor Yellow
}

# Apply database migrations
Write-Host ""
Write-Host "📊 Applying database migrations..." -ForegroundColor Yellow
Set-Location -Path "services/api/Api"

# Check if migrations exist
$migrationsExist = Test-Path "Migrations"

if ($migrationsExist) {
    Write-Host "ℹ️  Migrations already exist" -ForegroundColor Cyan
    $applyMigrations = Read-Host "Do you want to apply migrations? (Y/N)"
    if ($applyMigrations -eq "Y" -or $applyMigrations -eq "y") {
        dotnet ef database update
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Migrations applied successfully" -ForegroundColor Green
        }
        else {
            Write-Host "⚠️  Error applying migrations" -ForegroundColor Yellow
        }
    }
}
else {
    Write-Host "ℹ️  No migrations found. Creating initial migration..." -ForegroundColor Cyan
    dotnet ef migrations add InitialCreate
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Migration created successfully" -ForegroundColor Green
        dotnet ef database update
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Migration applied successfully" -ForegroundColor Green
        }
    }
}

Set-Location -Path "../../.."

# Display connection information
Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "🎉 Fluxora environment initialized successfully!" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "📝 Connection information:" -ForegroundColor White
Write-Host ""
Write-Host "🔑 Keycloak Admin Console:" -ForegroundColor Yellow
Write-Host "   URL: http://localhost:8080" -ForegroundColor White
Write-Host "   Username: admin" -ForegroundColor White
Write-Host "   Password: admin" -ForegroundColor White
Write-Host ""
Write-Host "🗄️  PostgreSQL:" -ForegroundColor Yellow
Write-Host "   Host: localhost" -ForegroundColor White
Write-Host "   Port: 5432" -ForegroundColor White
Write-Host "   Database: fluxora_dev" -ForegroundColor White
Write-Host "   Username: postgres" -ForegroundColor White
Write-Host "   Password: postgres" -ForegroundColor White
Write-Host ""
Write-Host "🖥️  pgAdmin:" -ForegroundColor Yellow
Write-Host "   URL: http://localhost:5050" -ForegroundColor White
Write-Host "   Email: admin@fluxora.dev" -ForegroundColor White
Write-Host "   Password: admin" -ForegroundColor White
Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "📚 Next steps:" -ForegroundColor Yellow
Write-Host "   1. Configure Keycloak following KEYCLOAK_SETUP.md" -ForegroundColor White
Write-Host "   2. Start the API with: cd services/api/Api && dotnet run" -ForegroundColor White
Write-Host "   3. Access Swagger UI: https://localhost:5001/swagger" -ForegroundColor White
Write-Host ""
Write-Host "💡 To stop services: docker-compose down" -ForegroundColor Cyan
Write-Host ""
