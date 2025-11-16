# Verify-WebConfig.ps1
# Verifies that web.config files have correct environment variables for Azure Arc authentication
#
# HOW DEFAULTAZURECREDENTIAL FINDS ARC IDENTITY:
# ================================================
# DefaultAzureCredential tries authentication methods in this order:
#   1. EnvironmentCredential - Reads AZURE_CLIENT_ID, AZURE_TENANT_ID, AZURE_CLIENT_SECRET
#   2. WorkloadIdentityCredential - Kubernetes workload identity
#   3. ManagedIdentityCredential - ⭐ Azure Arc / VM managed identity (THIS ONE!)
#   4. SharedTokenCacheCredential - Visual Studio token cache
#   5. VisualStudioCredential - Visual Studio account
#   6. AzureCliCredential - Azure CLI login
#   7. AzurePowerShellCredential - Azure PowerShell login
#
# For ManagedIdentityCredential to work on Azure Arc:
#   - HIMDS service must be running (Get-Service himds)
#   - Arc agent must be connected (azcmagent show)
#   - ⭐ AZURE_CLIENT_ID environment variable MUST be set to the Arc Principal ID
#   - App pool must be recycled after setting environment variables
#
# Without AZURE_CLIENT_ID, you get:
#   "EnvironmentCredential authentication unavailable"
#   "ManagedIdentityCredential authentication unavailable"
#
# With AZURE_CLIENT_ID correctly set:
#   ManagedIdentityCredential → HIMDS (localhost:40342) → Access Token ✅

param(
    [Parameter(Mandatory=$false)]
    [string]$WebAppPath = "E:\aidcircle\web",
    
    [Parameter(Mandatory=$false)]
    [string]$ApiAppPath = "E:\aidcircle\api"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Web.Config Verification" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

function Test-EnvironmentVariable {
    param(
        [xml]$config,
        [string]$varName,
        [string]$appType
    )
    
    $aspNetCore = $null
    if ($config.configuration.location) {
        $aspNetCore = $config.configuration.location.'system.webServer'.aspNetCore
    } else {
        $aspNetCore = $config.configuration.'system.webServer'.aspNetCore
    }
    
    if ($null -eq $aspNetCore) {
        Write-Host "❌ [$appType] Could not find aspNetCore element" -ForegroundColor Red
        return $false
    }
    
    $envVarsNode = $aspNetCore.SelectSingleNode("environmentVariables")
    if ($null -eq $envVarsNode) {
        Write-Host "❌ [$appType] No environmentVariables section found" -ForegroundColor Red
        return $false
    }
    
    $envVar = $envVarsNode.SelectSingleNode("environmentVariable[@name='$varName']")
    if ($null -eq $envVar) {
        Write-Host "❌ [$appType] Missing: $varName" -ForegroundColor Red
        return $false
    }
    
    $value = $envVar.GetAttribute("value")
    if ([string]::IsNullOrEmpty($value)) {
        Write-Host "⚠️  [$appType] $varName is empty" -ForegroundColor Yellow
        return $false
    }
    
    Write-Host "✅ [$appType] $varName = $value" -ForegroundColor Green
    return $true
}

# Check Web App
Write-Host "Checking Web Application..." -ForegroundColor Yellow
$webConfigPath = Join-Path $WebAppPath "web.config"

if (Test-Path $webConfigPath) {
    [xml]$webConfig = Get-Content $webConfigPath
    
    $webVars = @(
        "AZURE_CLIENT_ID",
        "KeyVaultName",
        "ASPNETCORE_ENVIRONMENT",
        "AzureOpenAI__Endpoint",
        "AzureAdB2C__Instance",
        "ApiSettings__BaseUrl"
    )
    
    $webOk = $true
    foreach ($var in $webVars) {
        if (!(Test-EnvironmentVariable -config $webConfig -varName $var -appType "WEB")) {
            $webOk = $false
        }
    }
    
    if ($webOk) {
        Write-Host ""
        Write-Host "✅ Web app configuration looks good!" -ForegroundColor Green
    } else {
        Write-Host ""
        Write-Host "❌ Web app configuration has issues. Re-run Configure-IISArcIdentity.ps1" -ForegroundColor Red
    }
} else {
    Write-Host "❌ Web app web.config not found at: $webConfigPath" -ForegroundColor Red
}

Write-Host ""
Write-Host "Checking API Application..." -ForegroundColor Yellow
$apiConfigPath = Join-Path $ApiAppPath "web.config"

if (Test-Path $apiConfigPath) {
    [xml]$apiConfig = Get-Content $apiConfigPath
    
    $apiVars = @(
        "AZURE_CLIENT_ID",
        "KeyVaultName",
        "ASPNETCORE_ENVIRONMENT",
        "AzureOpenAI__Endpoint"
    )
    
    $apiOk = $true
    foreach ($var in $apiVars) {
        if (!(Test-EnvironmentVariable -config $apiConfig -varName $var -appType "API")) {
            $apiOk = $false
        }
    }
    
    if ($apiOk) {
        Write-Host ""
        Write-Host "✅ API app configuration looks good!" -ForegroundColor Green
    } else {
        Write-Host ""
        Write-Host "❌ API app configuration has issues. Re-run Configure-IISArcIdentity.ps1" -ForegroundColor Red
    }
} else {
    Write-Host "❌ API app web.config not found at: $apiConfigPath" -ForegroundColor Red
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Next: Test Managed Identity" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "How DefaultAzureCredential Uses AZURE_CLIENT_ID:" -ForegroundColor Cyan
Write-Host "1. App calls DefaultAzureCredential.GetTokenAsync()" -ForegroundColor Gray
Write-Host "2. Tries EnvironmentCredential (checks AZURE_CLIENT_ID exists)" -ForegroundColor Gray
Write-Host "3. If EnvironmentCredential fails, tries ManagedIdentityCredential" -ForegroundColor Gray
Write-Host "4. ManagedIdentityCredential → http://localhost:40342 (HIMDS endpoint)" -ForegroundColor Gray
Write-Host "5. HIMDS uses AZURE_CLIENT_ID to identify which Arc identity to use" -ForegroundColor Gray
Write-Host "6. Returns access token for Azure resources (Key Vault, SQL, etc.)" -ForegroundColor Gray
Write-Host ""
Write-Host "⚠️  IMPORTANT: HIMDS requires client_id parameter in the request!" -ForegroundColor Yellow
Write-Host ""

# Try to extract AZURE_CLIENT_ID from web.config for the test
$testClientId = $null
if (Test-Path $webConfigPath) {
    try {
        [xml]$testConfig = Get-Content $webConfigPath
        $aspNetCore = if ($testConfig.configuration.location) { 
            $testConfig.configuration.location.'system.webServer'.aspNetCore 
        } else { 
            $testConfig.configuration.'system.webServer'.aspNetCore 
        }
        $envVars = $aspNetCore.SelectSingleNode("environmentVariables")
        $clientIdVar = $envVars.SelectSingleNode("environmentVariable[@name='AZURE_CLIENT_ID']")
        if ($clientIdVar) {
            $testClientId = $clientIdVar.GetAttribute("value")
        }
    } catch {
        # Ignore errors, will use default message
    }
}

Write-Host "Test the managed identity endpoint:" -ForegroundColor White
if ($testClientId) {
    Write-Host ""
    Write-Host "# Using AZURE_CLIENT_ID from your web.config:" -ForegroundColor Green
    Write-Host "`$clientId = `"$testClientId`"" -ForegroundColor Gray
    Write-Host '# Build URL with client_id parameter (REQUIRED for Arc):' -ForegroundColor Green
    Write-Host '$url = "http://localhost:40342/metadata/identity/oauth2/token?api-version=2020-06-01&resource=https://vault.azure.net&client_id=$clientId"' -ForegroundColor Gray
    Write-Host '$response = Invoke-RestMethod -Uri $url -Headers @{Metadata="true"} -Method GET' -ForegroundColor Gray
    Write-Host '$response | ConvertTo-Json' -ForegroundColor Gray
    Write-Host ""
    Write-Host "Quick test (copy/paste this):" -ForegroundColor Cyan
    Write-Host "`$url = `"http://localhost:40342/metadata/identity/oauth2/token?api-version=2020-06-01&resource=https://vault.azure.net&client_id=$testClientId`"" -ForegroundColor White
    Write-Host 'Invoke-RestMethod -Uri $url -Headers @{Metadata="true"} -Method GET | ConvertTo-Json' -ForegroundColor White
} else {
    Write-Host '# Replace YOUR_PRINCIPAL_ID with the AZURE_CLIENT_ID from web.config above' -ForegroundColor Yellow
    Write-Host '$clientId = "YOUR_PRINCIPAL_ID"  # e.g., cc261686-88bf-4252-84c1-44b28dd1c533' -ForegroundColor Gray
    Write-Host '$url = "http://localhost:40342/metadata/identity/oauth2/token?api-version=2020-06-01&resource=https://vault.azure.net&client_id=$clientId"' -ForegroundColor Gray
    Write-Host '$response = Invoke-RestMethod -Uri $url -Headers @{Metadata="true"} -Method GET' -ForegroundColor Gray
    Write-Host '$response | ConvertTo-Json' -ForegroundColor Gray
}
Write-Host ""
Write-Host "Expected response: JSON with access_token, expires_on, token_type" -ForegroundColor Green
Write-Host ""
Write-Host "❌ Common Error: Missing client_id parameter" -ForegroundColor Red
Write-Host '   Error: "unauthorized_client", "Missing Basic Authorization header"' -ForegroundColor Gray
Write-Host '   Fix: Add &client_id=YOUR_PRINCIPAL_ID to the URL' -ForegroundColor Gray
Write-Host ""
Write-Host "If this fails, check:" -ForegroundColor Yellow
Write-Host "1. HIMDS service: Get-Service himds" -ForegroundColor Gray
Write-Host "2. Arc agent status: azcmagent show" -ForegroundColor Gray
Write-Host "3. AZURE_CLIENT_ID is set above (should be ✅)" -ForegroundColor Gray
Write-Host "4. App pools recycled after web.config changes" -ForegroundColor Gray
Write-Host "5. client_id parameter included in URL" -ForegroundColor Gray
