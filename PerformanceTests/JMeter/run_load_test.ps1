<#
.SYNOPSIS
    Automated JMeter test runner and HTML report generator for Quiz_Web.
.EXAMPLE
    .\run_load_test.ps1 -Target "online-learning-system-543q.onrender.com" -Protocol "https" -Users 30 -Duration 60
    .\run_load_test.ps1 -Target "localhost" -Port "5000" -Protocol "http" -Users 10 -Duration 30
#>
param(
    [string]$Target = "online-learning-system-543q.onrender.com",
    [string]$Port = "",
    [string]$Protocol = "https",
    [int]$Users = 20,
    [int]$RampUp = 10,
    [int]$Duration = 60,
    [string]$JMeterBinPath = ""
)

$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $ScriptDir

# Locate JMeter executable
$JMeterCmd = "jmeter"
if ($JMeterBinPath -and (Test-Path "$JMeterBinPath\jmeter.bat")) {
    $JMeterCmd = "$JMeterBinPath\jmeter.bat"
} elseif (-not (Get-Command "jmeter" -ErrorAction SilentlyContinue)) {
    # Search common install paths
    $commonPaths = @(
        "C:\apache-jmeter-*\bin\jmeter.bat",
        "$env:USERPROFILE\apache-jmeter-*\bin\jmeter.bat",
        "C:\Tools\apache-jmeter-*\bin\jmeter.bat",
        "D:\apache-jmeter-*\bin\jmeter.bat"
    )
    $found = Get-ChildItem -Path $commonPaths -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($found) {
        $JMeterCmd = $found.FullName
    } else {
        Write-Warning "JMeter is not found in PATH."
        Write-Host "Please install Apache JMeter or specify -JMeterBinPath 'C:\path\to\apache-jmeter\bin'." -ForegroundColor Yellow
        Write-Host "You can download JMeter from: https://jmeter.apache.org/download_jmeter.cgi" -ForegroundColor Cyan
        exit 1
    }
}

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "        QUIZ_WEB JMETER PERFORMANCE TEST RUNNER           " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "Target Host : $Target"
Write-Host "Port        : $(if ($Port) { $Port } else { '(Default)' })"
Write-Host "Protocol    : $Protocol"
Write-Host "Concurrency : $Users concurrent users"
Write-Host "Ramp-up     : $RampUp seconds"
Write-Host "Duration    : $Duration seconds"
Write-Host "Using JMeter: $JMeterCmd"
Write-Host "----------------------------------------------------------"

$TestPlan = Join-Path $ScriptDir "load_test_plan.jmx"
$ResultLog = Join-Path $ScriptDir "results.jtl"
$ReportDir = Join-Path $ScriptDir "report"

# Cleanup previous run artifacts
if (Test-Path $ResultLog) { Remove-Item -Force $ResultLog }
if (Test-Path $ReportDir) { Remove-Item -Recurse -Force $ReportDir }

$cmdArgs = @(
    "-n",
    "-t", "`"$TestPlan`"",
    "-l", "`"$ResultLog`"",
    "-e",
    "-o", "`"$ReportDir`"",
    "-JBASE_URL=$Target",
    "-JPORT=$Port",
    "-JPROTOCOL=$Protocol",
    "-JTHREADS=$Users",
    "-JRAMP_TIME=$RampUp",
    "-JDURATION=$Duration"
)

Write-Host "Executing JMeter in Non-GUI Mode..." -ForegroundColor Green
$process = Start-Process -FilePath $JMeterCmd -ArgumentList $cmdArgs -NoNewWindow -Wait -PassThru

if ($process.ExitCode -eq 0) {
    Write-Host "`nTest completed successfully!" -ForegroundColor Green
    $HtmlIndex = Join-Path $ReportDir "index.html"
    if (Test-Path $HtmlIndex) {
        Write-Host "HTML Dashboard Report generated at:" -ForegroundColor Cyan
        Write-Host "$HtmlIndex" -ForegroundColor Yellow
        Write-Host "Opening report in default browser..."
        Start-Process $HtmlIndex
    }
} else {
    Write-Host "`nJMeter execution exited with code $($process.ExitCode)" -ForegroundColor Red
}
