# Diagnose-ArcAgent.ps1
# Comprehensive diagnostic script for Azure Arc agent and HIMDS issues

Write-Host "=== Azure Arc Agent Diagnostics ===" -ForegroundColor Cyan
Write-Host ""

# Check 1: Arc Agent Status
Write-Host "[1/8] Checking Arc Agent Status..." -ForegroundColor Yellow
try {
    $arcStatus = azcmagent show 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Arc Agent Found" -ForegroundColor Green
        Write-Host $arcStatus
        Write-Host ""
    } else {
        Write-Host "❌ Arc Agent Not Found or Not Configured" -ForegroundColor Red
        Write-Host "   Run: azcmagent connect --help" -ForegroundColor Yellow
        Write-Host ""
    }
} catch {
    Write-Host "❌ azcmagent command not found. Arc agent may not be installed." -ForegroundColor Red
    Write-Host ""
}

# Check 2: HIMDS Service Status
Write-Host "[2/8] Checking HIMDS Service..." -ForegroundColor Yellow
try {
    $himdsService = Get-Service himds -ErrorAction Stop
    if ($himdsService.Status -eq 'Running') {
        Write-Host "✅ HIMDS Service Running" -ForegroundColor Green
        Write-Host "   Status: $($himdsService.Status)" -ForegroundColor Gray
        Write-Host "   StartType: $($himdsService.StartType)" -ForegroundColor Gray
    } else {
        Write-Host "⚠️  HIMDS Service Not Running" -ForegroundColor Yellow
        Write-Host "   Status: $($himdsService.Status)" -ForegroundColor Yellow
        Write-Host "   Try: Restart-Service himds (as Administrator)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "❌ HIMDS Service Not Found" -ForegroundColor Red
    Write-Host "   Arc agent may not be properly installed" -ForegroundColor Red
}
Write-Host ""

# Check 3: HIMDS Network Endpoint
Write-Host "[3/8] Checking HIMDS Network Endpoint..." -ForegroundColor Yellow
try {
    $netTest = Test-NetConnection -ComputerName localhost -Port 40342 -WarningAction SilentlyContinue
    if ($netTest.TcpTestSucceeded) {
        Write-Host "✅ HIMDS Listening on localhost:40342" -ForegroundColor Green
    } else {
        Write-Host "❌ HIMDS Not Listening on localhost:40342" -ForegroundColor Red
        Write-Host "   HIMDS service may be stopped or failed to start" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Network Test Failed" -ForegroundColor Red
}
Write-Host ""

# Check 4: Extract Principal ID from Azure
Write-Host "[4/8] Checking Principal ID from Azure..." -ForegroundColor Yellow
Write-Host "   (Requires Azure CLI and login to KFAM1 subscription)" -ForegroundColor Gray
try {
    # Store current subscription
    $currentSub = az account show --query id -o tsv 2>$null
    
    # Try to get Arc machine info
    $principalId = az connectedmachine show --name "WEB1" --resource-group "rg-ss-vms" --subscription "KFAM1" --query "identity.principalId" -o tsv 2>$null
    
    if ($LASTEXITCODE -eq 0 -and $principalId) {
        Write-Host "✅ Principal ID from Azure: $principalId" -ForegroundColor Green
        Write-Host "   Local test using: cc261686-88bf-4252-84c1-44b28dd1c533" -ForegroundColor Gray
        
        if ($principalId -eq "cc261686-88bf-4252-84c1-44b28dd1c533") {
            Write-Host "   ✅ MATCH - Principal IDs match" -ForegroundColor Green
        } else {
            Write-Host "   ❌ MISMATCH - Update your test with correct Principal ID" -ForegroundColor Red
        }
    } else {
        Write-Host "⚠️  Could not retrieve Principal ID from Azure" -ForegroundColor Yellow
        Write-Host "   Make sure you're logged in: az login" -ForegroundColor Yellow
        Write-Host "   And have access to KFAM1 subscription" -ForegroundColor Yellow
    }
    
    # Restore subscription
    if ($currentSub) {
        az account set --subscription $currentSub 2>$null
    }
} catch {
    Write-Host "⚠️  Azure CLI check failed" -ForegroundColor Yellow
}
Write-Host ""

# Check 5: Test HIMDS Endpoint with client_id
Write-Host "[5/8] Testing HIMDS Endpoint (with client_id)..." -ForegroundColor Yellow
$clientId = "cc261686-88bf-4252-84c1-44b28dd1c533"
$url = "http://localhost:40342/metadata/identity/oauth2/token?api-version=2020-06-01&resource=https://vault.azure.net&client_id=$clientId"

try {
    $response = Invoke-RestMethod -Uri $url -Headers @{Metadata="true"} -Method GET -ErrorAction Stop
    Write-Host "✅ HIMDS Endpoint Responding Successfully" -ForegroundColor Green
    Write-Host "   Token Type: $($response.token_type)" -ForegroundColor Gray
    Write-Host "   Expires In: $($response.expires_in) seconds" -ForegroundColor Gray
    Write-Host "   Resource: $($response.resource)" -ForegroundColor Gray
    Write-Host ""
    Write-Host "   🎉 Managed Identity Authentication Working!" -ForegroundColor Green
    Write-Host ""
} catch {
    $errorResponse = $_.ErrorDetails.Message | ConvertFrom-Json
    Write-Host "❌ HIMDS Endpoint Failed" -ForegroundColor Red
    Write-Host "   Error: $($errorResponse.error)" -ForegroundColor Red
    Write-Host "   Description: $($errorResponse.error_description)" -ForegroundColor Red
    Write-Host ""
    
    if ($errorResponse.error -eq "unauthorized_client") {
        Write-Host "   🔍 Troubleshooting 'unauthorized_client':" -ForegroundColor Yellow
        Write-Host "   1. Restart HIMDS service: Restart-Service himds" -ForegroundColor Yellow
        Write-Host "   2. Check Arc agent connection: azcmagent show" -ForegroundColor Yellow
        Write-Host "   3. Verify Principal ID matches Azure" -ForegroundColor Yellow
        Write-Host "   4. If all fails, reconnect Arc agent: azcmagent disconnect && azcmagent connect" -ForegroundColor Yellow
    }
}
Write-Host ""

# Check 6: Web Config Files
Write-Host "[6/8] Checking Web Config Files..." -ForegroundColor Yellow

$webConfigPath = "E:\aidcircle\web\web.config"
$apiConfigPath = "E:\aidcircle\api\web.config"

function Check-WebConfig {
    param($path, $appName)
    
    if (Test-Path $path) {
        Write-Host "   Checking $appName..." -ForegroundColor Gray
        $xml = [xml](Get-Content $path)
        $envVars = $xml.configuration.location.system.webServer.aspNetCore.environmentVariables.environmentVariable
        
        $clientIdVar = $envVars | Where-Object { $_.name -eq "AZURE_CLIENT_ID" }
        
        if ($clientIdVar) {
            Write-Host "   ✅ AZURE_CLIENT_ID set: $($clientIdVar.value)" -ForegroundColor Green
        } else {
            Write-Host "   ❌ AZURE_CLIENT_ID NOT SET" -ForegroundColor Red
            Write-Host "      Run Configure-IISArcIdentity.ps1 to fix" -ForegroundColor Yellow
        }
    } else {
        Write-Host "   ⚠️  Config not found: $path" -ForegroundColor Yellow
    }
}

Check-WebConfig -path $webConfigPath -appName "Web App"
Check-WebConfig -path $apiConfigPath -appName "API App"
Write-Host ""

# Check 7: IIS App Pool Status
Write-Host "[7/8] Checking IIS App Pool Status..." -ForegroundColor Yellow
try {
    # Use Windows PowerShell for IIS operations
    $psPath = "$env:SystemRoot\System32\WindowsPowerShell\v1.0\powershell.exe"
    
    $webStatus = & $psPath -Command "Import-Module WebAdministration; (Get-WebAppPoolState -Name 'aidcircle-web').Value" 2>$null
    $apiStatus = & $psPath -Command "Import-Module WebAdministration; (Get-WebAppPoolState -Name 'aidcircle-api').Value" 2>$null
    
    if ($webStatus) {
        Write-Host "   Web App Pool: $webStatus" -ForegroundColor $(if ($webStatus -eq 'Started') { 'Green' } else { 'Yellow' })
    } else {
        Write-Host "   ⚠️  Could not get web app pool status" -ForegroundColor Yellow
    }
    
    if ($apiStatus) {
        Write-Host "   API App Pool: $apiStatus" -ForegroundColor $(if ($apiStatus -eq 'Started') { 'Green' } else { 'Yellow' })
    } else {
        Write-Host "   ⚠️  Could not get API app pool status" -ForegroundColor Yellow
    }
} catch {
    Write-Host "   ⚠️  IIS check failed (may need Administrator)" -ForegroundColor Yellow
}
Write-Host ""

# Check 8: Key Vault Access
Write-Host "[8/8] Checking Key Vault Access Policy..." -ForegroundColor Yellow
try {
    $currentSub = az account show --query id -o tsv 2>$null
    
    $accessPolicy = az keyvault show --name "aidcirclekeyvault" --subscription "DEVTEST" --query "properties.accessPolicies[?objectId=='cc261686-88bf-4252-84c1-44b28dd1c533']" 2>$null | ConvertFrom-Json
    
    if ($accessPolicy) {
        Write-Host "✅ Key Vault Access Policy Found" -ForegroundColor Green
        Write-Host "   Permissions:" -ForegroundColor Gray
        Write-Host "   - Secrets: $($accessPolicy.permissions.secrets -join ', ')" -ForegroundColor Gray
    } else {
        Write-Host "❌ No Access Policy Found for Arc Identity" -ForegroundColor Red
        Write-Host "   Run Configure-IISArcIdentity.ps1 to grant access" -ForegroundColor Yellow
    }
    
    if ($currentSub) {
        az account set --subscription $currentSub 2>$null
    }
} catch {
    Write-Host "⚠️  Could not check Key Vault (Azure CLI required)" -ForegroundColor Yellow
}
Write-Host ""

# Summary
Write-Host "=== Summary & Next Steps ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "If HIMDS test failed with 'unauthorized_client':" -ForegroundColor Yellow
Write-Host "  1. Try restarting HIMDS: Restart-Service himds (as Administrator)" -ForegroundColor White
Write-Host "  2. Wait 30 seconds and run this script again" -ForegroundColor White
Write-Host "  3. If still failing, check Arc agent: azcmagent show" -ForegroundColor White
Write-Host "  4. Last resort: Reconnect Arc agent (see DIAGNOSE-HIMDS-ERROR.md)" -ForegroundColor White
Write-Host ""
Write-Host "Even if HIMDS test fails, your application may still work!" -ForegroundColor Green
Write-Host "Check application logs: Get-Content 'E:\aidcircle\web\logs\stdout_*.log' -Tail 50" -ForegroundColor Cyan
Write-Host ""
