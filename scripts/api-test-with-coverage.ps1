<#
.SYNOPSIS
    Runs unit tests with code coverage and generates HTML report
.DESCRIPTION
    Executes all unit tests, collects coverage data, and generates an HTML report using ReportGenerator
#>

param(
    [switch]$OpenReport,
    [string]$Filter = ""
)

$ErrorActionPreference = "Stop"

Write-Host "🧪 Running unit tests with coverage..." -ForegroundColor Cyan

# Run tests with coverage
$testArgs = @(
    "test",
    "services/api/Api.Tests/Api.Tests.csproj",
    "--collect:XPlat Code Coverage",
    "--results-directory", "services/api/Api.Tests/TestResults",
    "--logger", "console;verbosity=normal"
)

if ($Filter) {
    $testArgs += "--filter"
    $testArgs += $Filter
}

& dotnet $testArgs

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Tests failed" -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host "✅ Tests passed" -ForegroundColor Green

# Check if ReportGenerator is installed
Write-Host "📊 Generating coverage report..." -ForegroundColor Cyan

$reportGeneratorPath = (Get-Command reportgenerator -ErrorAction SilentlyContinue).Source

if (-not $reportGeneratorPath) {
    Write-Host "⚙️ Installing ReportGenerator..." -ForegroundColor Yellow
    dotnet tool install -g dotnet-reportgenerator-globaltool
}

# Generate HTML report
$coverageFiles = Get-ChildItem -Path "services/api/Api.Tests/TestResults/*/coverage.cobertura.xml" -Recurse
if ($coverageFiles.Count -eq 0) {
    Write-Host "⚠️ No coverage files found" -ForegroundColor Yellow
    exit 0
}

$reportPath = "coverage-report"
& reportgenerator `
    "-reports:services/api/Api.Tests/TestResults/*/coverage.cobertura.xml" `
    "-targetdir:$reportPath" `
    "-reporttypes:Html;Badges" `
    "-historydir:coverage-history" `
    "-classfilters:-*.DTOs.*;-*.Migrations.*" `
    "-filefilters:-**/DTOs/**;-**/Migrations/**;-**/Program.cs;-**/*.Designer.cs"

Write-Host "✅ Coverage report generated at: $reportPath/index.html" -ForegroundColor Green

# Open report in browser
if ($OpenReport) {
    Start-Process "$reportPath/index.html"
}

# Display coverage summary
Write-Host "`n📈 Coverage Summary:" -ForegroundColor Cyan
$summaryFile = Get-Content "$reportPath/Summary.txt" -ErrorAction SilentlyContinue
if ($summaryFile) {
    Write-Host $summaryFile -ForegroundColor White
}