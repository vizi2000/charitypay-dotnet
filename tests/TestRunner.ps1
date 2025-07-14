# CharityPay .NET Test Runner
# Comprehensive test execution script with coverage and reporting

param(
    [string]$TestType = "All",          # All, Unit, Integration, Performance
    [string]$Project = "All",           # All, Domain, Application, Infrastructure, API
    [switch]$Coverage = $false,         # Enable code coverage
    [switch]$Verbose = $false,          # Verbose output
    [switch]$Parallel = $true,          # Run tests in parallel
    [string]$OutputPath = "./TestResults", # Output directory
    [switch]$OpenResults = $false       # Open results in browser
)

# Colors for output
$Red = "Red"
$Green = "Green"
$Yellow = "Yellow"
$Blue = "Blue"
$Cyan = "Cyan"

function Write-Header {
    param([string]$Message)
    Write-Host "`n$('=' * 60)" -ForegroundColor $Blue
    Write-Host $Message -ForegroundColor $Blue
    Write-Host $('=' * 60) -ForegroundColor $Blue
}

function Write-Success {
    param([string]$Message)
    Write-Host "✓ $Message" -ForegroundColor $Green
}

function Write-Warning {
    param([string]$Message)
    Write-Host "⚠ $Message" -ForegroundColor $Yellow
}

function Write-Error {
    param([string]$Message)
    Write-Host "✗ $Message" -ForegroundColor $Red
}

function Write-Info {
    param([string]$Message)
    Write-Host "ℹ $Message" -ForegroundColor $Cyan
}

# Ensure output directory exists
if (!(Test-Path $OutputPath)) {
    New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
}

Write-Header "CharityPay .NET Test Runner"

# Check .NET installation
try {
    $dotnetVersion = dotnet --version
    Write-Info "Using .NET version: $dotnetVersion"
}
catch {
    Write-Error ".NET is not installed or not in PATH"
    exit 1
}

# Define test projects
$testProjects = @{
    "Domain" = "tests/CharityPay.Domain.Tests/CharityPay.Domain.Tests.csproj"
    "Application" = "tests/CharityPay.Application.Tests/CharityPay.Application.Tests.csproj"
    "Infrastructure" = "tests/CharityPay.Infrastructure.Tests/CharityPay.Infrastructure.Tests.csproj"
    "API" = "tests/CharityPay.API.Tests/CharityPay.API.Tests.csproj"
}

# Define test categories
$testCategories = @{
    "Unit" = "Category=Unit"
    "Integration" = "Category=Integration"
    "Performance" = "Speed=Slow"
}

function Get-ProjectsToRun {
    param([string]$ProjectFilter)
    
    if ($ProjectFilter -eq "All") {
        return $testProjects.Values
    }
    
    if ($testProjects.ContainsKey($ProjectFilter)) {
        return @($testProjects[$ProjectFilter])
    }
    
    Write-Warning "Unknown project: $ProjectFilter. Available: $($testProjects.Keys -join ', ')"
    return @()
}

function Get-TestFilter {
    param([string]$TestTypeFilter)
    
    switch ($TestTypeFilter) {
        "Unit" { return $testCategories["Unit"] }
        "Integration" { return $testCategories["Integration"] }
        "Performance" { return $testCategories["Performance"] }
        "All" { return $null }
        default { 
            Write-Warning "Unknown test type: $TestTypeFilter. Available: Unit, Integration, Performance, All"
            return $null
        }
    }
}

function Run-Tests {
    param(
        [string[]]$Projects,
        [string]$Filter,
        [bool]$EnableCoverage,
        [bool]$VerboseOutput,
        [bool]$RunParallel
    )
    
    $totalProjects = $Projects.Count
    $passedProjects = 0
    $failedProjects = 0
    
    foreach ($project in $Projects) {
        $projectName = Split-Path $project -LeafBase
        Write-Info "Running tests for $projectName..."
        
        # Build test command
        $testArgs = @("test", $project)
        
        # Add test filter if specified
        if ($Filter) {
            $testArgs += "--filter", $Filter
        }
        
        # Add logger for test results
        $testResultsFile = Join-Path $OutputPath "$projectName-results.trx"
        $testArgs += "--logger", "trx;LogFileName=$testResultsFile"
        
        # Add console logger with appropriate verbosity
        if ($VerboseOutput) {
            $testArgs += "--logger", "console;verbosity=detailed"
        } else {
            $testArgs += "--logger", "console;verbosity=normal"
        }
        
        # Add coverage if enabled
        if ($EnableCoverage) {
            $coverageFile = Join-Path $OutputPath "$projectName-coverage.xml"
            $testArgs += "--collect", "XPlat Code Coverage"
            $testArgs += "--results-directory", $OutputPath
        }
        
        # Add parallel execution
        if ($RunParallel) {
            $testArgs += "--parallel"
        }
        
        # Add no-build if not first run (optimization)
        $testArgs += "--no-build"
        
        try {
            Write-Host "Command: dotnet $($testArgs -join ' ')" -ForegroundColor Gray
            
            $process = Start-Process -FilePath "dotnet" -ArgumentList $testArgs -Wait -PassThru -NoNewWindow
            
            if ($process.ExitCode -eq 0) {
                Write-Success "Tests passed for $projectName"
                $passedProjects++
            } else {
                Write-Error "Tests failed for $projectName (Exit code: $($process.ExitCode))"
                $failedProjects++
            }
        }
        catch {
            Write-Error "Error running tests for $projectName`: $($_.Exception.Message)"
            $failedProjects++
        }
    }
    
    return @{
        Total = $totalProjects
        Passed = $passedProjects
        Failed = $failedProjects
    }
}

function Generate-CoverageReport {
    param([string]$OutputDir)
    
    Write-Info "Generating coverage report..."
    
    # Find coverage files
    $coverageFiles = Get-ChildItem -Path $OutputDir -Filter "coverage.cobertura.xml" -Recurse
    
    if ($coverageFiles.Count -eq 0) {
        Write-Warning "No coverage files found"
        return
    }
    
    # Check if reportgenerator is installed
    try {
        $null = Get-Command reportgenerator -ErrorAction Stop
    }
    catch {
        Write-Info "Installing ReportGenerator tool..."
        dotnet tool install --global dotnet-reportgenerator-globaltool
    }
    
    try {
        $coverageReportDir = Join-Path $OutputDir "coverage-report"
        $coveragePattern = ($coverageFiles | ForEach-Object { $_.FullName }) -join ";"
        
        reportgenerator "-reports:$coveragePattern" "-targetdir:$coverageReportDir" "-reporttypes:Html;Badges;TextSummary"
        
        Write-Success "Coverage report generated at: $coverageReportDir"
        
        # Display summary if available
        $summaryFile = Join-Path $coverageReportDir "Summary.txt"
        if (Test-Path $summaryFile) {
            Write-Host "`nCoverage Summary:" -ForegroundColor $Blue
            Get-Content $summaryFile | Write-Host
        }
        
        return $coverageReportDir
    }
    catch {
        Write-Error "Failed to generate coverage report: $($_.Exception.Message)"
    }
}

function Show-TestSummary {
    param([hashtable]$Results)
    
    Write-Header "Test Results Summary"
    
    Write-Host "Total Projects: $($Results.Total)" -ForegroundColor $Blue
    Write-Host "Passed: $($Results.Passed)" -ForegroundColor $Green
    Write-Host "Failed: $($Results.Failed)" -ForegroundColor $Red
    
    $successRate = if ($Results.Total -gt 0) { 
        [math]::Round(($Results.Passed / $Results.Total) * 100, 2) 
    } else { 
        0 
    }
    
    Write-Host "Success Rate: $successRate%" -ForegroundColor $(if ($successRate -ge 80) { $Green } elseif ($successRate -ge 60) { $Yellow } else { $Red })
    
    if ($Results.Failed -gt 0) {
        Write-Warning "Some tests failed. Check the detailed output above for more information."
        exit 1
    } else {
        Write-Success "All tests passed!"
    }
}

# Main execution
try {
    Write-Info "Test Configuration:"
    Write-Host "  Test Type: $TestType" -ForegroundColor Gray
    Write-Host "  Project: $Project" -ForegroundColor Gray
    Write-Host "  Coverage: $Coverage" -ForegroundColor Gray
    Write-Host "  Verbose: $Verbose" -ForegroundColor Gray
    Write-Host "  Parallel: $Parallel" -ForegroundColor Gray
    Write-Host "  Output Path: $OutputPath" -ForegroundColor Gray
    
    # Get projects to run
    $projectsToRun = Get-ProjectsToRun -ProjectFilter $Project
    
    if ($projectsToRun.Count -eq 0) {
        Write-Error "No projects to run"
        exit 1
    }
    
    # Get test filter
    $testFilter = Get-TestFilter -TestTypeFilter $TestType
    
    # Build solution first
    Write-Info "Building solution..."
    $buildResult = dotnet build --configuration Release
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Build failed"
        exit 1
    }
    Write-Success "Build completed"
    
    # Run tests
    $testResults = Run-Tests -Projects $projectsToRun -Filter $testFilter -EnableCoverage $Coverage -VerboseOutput $Verbose -RunParallel $Parallel
    
    # Generate coverage report if enabled
    if ($Coverage) {
        $coverageReportPath = Generate-CoverageReport -OutputDir $OutputPath
        
        if ($OpenResults -and $coverageReportPath) {
            $indexPath = Join-Path $coverageReportPath "index.html"
            if (Test-Path $indexPath) {
                Write-Info "Opening coverage report..."
                Start-Process $indexPath
            }
        }
    }
    
    # Show summary
    Show-TestSummary -Results $testResults
}
catch {
    Write-Error "Unexpected error: $($_.Exception.Message)"
    Write-Host $_.ScriptStackTrace -ForegroundColor $Red
    exit 1
}