<#
.SYNOPSIS
    Downloads and extracts Apache JMeter 5.6.3 to D:\apache-jmeter-5.6.3
#>
$ErrorActionPreference = "Stop"

$InstallDir = "D:\apache-jmeter-5.6.3"
$ZipPath = "$env:TEMP\apache-jmeter-5.6.3.zip"
$Url = "https://dlcdn.apache.org/jmeter/binaries/apache-jmeter-5.6.3.zip"

if (Test-Path "$InstallDir\bin\jmeter.bat") {
    Write-Host "Apache JMeter is already installed at: $InstallDir" -ForegroundColor Green
    exit 0
}

Write-Host "Downloading Apache JMeter 5.6.3 (approx. 86MB)..." -ForegroundColor Cyan
curl.exe -L -o "$ZipPath" "$Url"

Write-Host "Extracting to D:\..." -ForegroundColor Cyan
Expand-Archive -Path "$ZipPath" -DestinationPath "D:\" -Force

Remove-Item "$ZipPath" -Force -ErrorAction SilentlyContinue

Write-Host "Apache JMeter 5.6.3 installed successfully at: $InstallDir" -ForegroundColor Green
Write-Host "You can run JMeter GUI by opening: $InstallDir\bin\jmeter.bat" -ForegroundColor Yellow
