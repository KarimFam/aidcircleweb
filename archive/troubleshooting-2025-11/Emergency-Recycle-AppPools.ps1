# EMERGENCY FIX - Recycle App Pools and Verify Environment
# Run this in PowerShell AS ADMINISTRATOR

Write-Host "=== EMERGENCY APP POOL RECYCLE ===" -ForegroundColor Red
Write-Host ""

# Import IIS module using Windows PowerShell
Write-Host "[1/5] Loading IIS Module..." -ForegroundColor Yellow
Import-Module WebAdministration -ErrorAction Stop
Write-Host "✅ IIS Module Loaded" -ForegroundColor Green
Write-Host ""

# Stop both app pools
Write-Host "[2/5] Stopping App Pools..." -ForegroundColor Yellow
try {
    Stop-WebAppPool -Name "aidcircle-web"
    Write-Host "✅ aidcircle-web stopped" -ForegroundColor Green
} catch {
    Write-Host "⚠️  Could not stop aidcircle-web: $_" -ForegroundColor Yellow
}

try {
    Stop-WebAppPool -Name "aidcircle-api"
    Write-Host "✅ aidcircle-api stopped" -ForegroundColor Green
} catch {
    Write-Host "⚠️  Could not stop aidcircle-api: $_" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Waiting 10 seconds for worker processes to fully terminate..." -ForegroundColor Gray
Start-Sleep -Seconds 10

# Kill any remaining worker processes
Write-Host "[3/5] Ensuring Worker Processes Terminated..." -ForegroundColor Yellow
$w3wpProcesses = Get-Process w3wp -ErrorAction SilentlyContinue
if ($w3wpProcesses) {
    Write-Host "Found $($w3wpProcesses.Count) w3wp.exe processes, terminating..." -ForegroundColor Yellow
    $w3wpProcesses | Stop-Process -Force
    Write-Host "✅ All w3wp.exe processes terminated" -ForegroundColor Green
} else {
    Write-Host "✅ No w3wp.exe processes found" -ForegroundColor Green
}

Write-Host ""
Write-Host "Waiting 5 seconds..." -ForegroundColor Gray
Start-Sleep -Seconds 5

# Start both app pools
Write-Host "[4/5] Starting App Pools..." -ForegroundColor Yellow
try {
    Start-WebAppPool -Name "aidcircle-web"
    Write-Host "✅ aidcircle-web started" -ForegroundColor Green
} catch {
    Write-Host "❌ Could not start aidcircle-web: $_" -ForegroundColor Red
}

try {
    Start-WebAppPool -Name "aidcircle-api"
    Write-Host "✅ aidcircle-api started" -ForegroundColor Green
} catch {
    Write-Host "❌ Could not start aidcircle-api: $_" -ForegroundColor Red
}

Write-Host ""
Write-Host "Waiting 10 seconds for applications to start..." -ForegroundColor Gray
Start-Sleep -Seconds 10

# Check app pool states
Write-Host "[5/5] Verifying App Pool States..." -ForegroundColor Yellow
$webState = (Get-WebAppPoolState -Name "aidcircle-web").Value
$apiState = (Get-WebAppPoolState -Name "aidcircle-api").Value

Write-Host "aidcircle-web: $webState" -ForegroundColor $(if ($webState -eq 'Started') { 'Green' } else { 'Red' })
Write-Host "aidcircle-api: $apiState" -ForegroundColor $(if ($apiState -eq 'Started') { 'Green' } else { 'Red' })

Write-Host ""
Write-Host "=== Next Steps ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. Check web app logs for errors:" -ForegroundColor White
Write-Host "   Get-Content 'E:\aidcircle\web\logs\stdout_*.log' -Tail 50" -ForegroundColor Gray
Write-Host ""
Write-Host "2. Test if web app responds:" -ForegroundColor White
Write-Host "   Invoke-WebRequest -Uri 'http://localhost:5011' -UseBasicParsing" -ForegroundColor Gray
Write-Host ""
Write-Host "3. If STILL getting ManagedIdentityCredential errors, the issue is NOT environment variables." -ForegroundColor Yellow
Write-Host "   The problem may be:" -ForegroundColor Yellow
Write-Host "   - Application configuration (appsettings.json)" -ForegroundColor Yellow
Write-Host "   - Missing Azure.Identity NuGet package" -ForegroundColor Yellow
Write-Host "   - Incorrect KeyVault configuration in code" -ForegroundColor Yellow
Write-Host ""
