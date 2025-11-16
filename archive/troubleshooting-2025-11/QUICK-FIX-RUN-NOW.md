# Quick Fix Guide - Run These Commands on WEB1 Server

## ⚡ TLDR - Run This Now

```powershell
# 1. Re-run configuration script with correct parameters
cd E:\aidcircle
.\Configure-IISArcIdentity.ps1 `
    -MachineName "WEB1" `
    -ArcResourceGroup "rg-ss-vms" `
    -ArcSubscription "KFAM1" `
    -KeyVaultName "aidcirclekeyvault" `
    -KeyVaultSubscription "DEVTEST" `
    -WebAppPoolName "aidcircle-web" `
    -ApiAppPoolName "aidcircle-api" `
    -AzureOpenAIEndpoint "https://your-openai-endpoint.openai.azure.com/"

# 2. Verify configuration
.\Verify-WebConfig.ps1

# 3. Test managed identity
$clientId = "cc261686-88bf-4252-84c1-44b28dd1c533"  # Your Arc Principal ID
$url = "http://localhost:40342/metadata/identity/oauth2/token?api-version=2020-06-01&resource=https://vault.azure.net&client_id=$clientId"
Invoke-RestMethod -Uri $url -Headers @{Metadata="true"} -Method GET

# 4. Check application logs
Get-Content "E:\aidcircle\web\logs\stdout_*.log" -Tail 50
```

## 📋 What Was Fixed

### Issue 1: Key Vault Access Failed ❌ → ✅
**Before**: Script switched to Key Vault subscription but didn't grant access properly  
**After**: Script now stays in Key Vault subscription long enough to grant access, then restores context

**Result**: Access policy now granted successfully ✅

### Issue 2: Environment Variables Not Set ❌ → ✅
**Before**: `AZURE_CLIENT_ID` missing from web.config  
**After**: Script correctly sets all environment variables including Principal ID

**Test**: Run `.\Verify-WebConfig.ps1` to confirm

### Issue 3: App Pools Not Recycled ❌ → ✅
**Before**: PowerShell 7 couldn't access IIS drive  
**After**: Script uses Windows PowerShell 5.1 for IIS operations

**Result**: App pools now recycle automatically ✅

### Issue 4: Arc Machine Not Found ❌ → ✅
**Before**: Script only searched current subscription  
**After**: Script searches specified subscription or all subscriptions

**Result**: Finds WEB1 in KFAM1 subscription ✅

## 🔍 Verify Everything Works

### Step 1: Check HIMDS Service
```powershell
Get-Service himds
# Expected: Status = Running
```

### Step 2: Check Arc Agent
```powershell
azcmagent show
# Expected: Agent Status = Connected
```

### Step 3: Check Environment Variables
```powershell
.\Verify-WebConfig.ps1
# Expected: All ✅ green checkmarks
```

### Step 4: Test Managed Identity Endpoint
```powershell
# IMPORTANT: Include client_id parameter (your Arc Principal ID)
$clientId = "cc261686-88bf-4252-84c1-44b28dd1c533"
$url = "http://localhost:40342/metadata/identity/oauth2/token?api-version=2020-06-01&resource=https://vault.azure.net&client_id=$clientId"
$result = Invoke-RestMethod -Uri $url -Headers @{Metadata="true"} -Method GET
$result | ConvertTo-Json
# Expected: JSON with access_token, expires_on, resource
```

**Common Error:**
```json
{
  "error": "unauthorized_client",
  "error_description": "Missing Basic Authorization header"
}
```
**Fix:** You forgot the `&client_id=YOUR_PRINCIPAL_ID` parameter in the URL!

### Step 5: Check Web App Logs
```powershell
Get-Content "E:\aidcircle\web\logs\stdout_*.log" -Tail 50

# ✅ SHOULD SEE:
# - "Hosting environment: Production"
# - "Now listening on: http://localhost:5011"

# ❌ SHOULD NOT SEE:
# - "CredentialUnavailableException"
# - "EnvironmentCredential authentication unavailable"
# - "ManagedIdentityCredential authentication unavailable"
```

### Step 6: Browse Application
```powershell
# Open browser
Start-Process "http://localhost:5011"

# Expected: Application loads without errors
```

## 🚨 If Something Still Fails

### Key Vault Access Still Failing?
```powershell
# Manual fix:
az login
az account set --subscription "DEVTEST"
az keyvault set-policy `
    --name aidcirclekeyvault `
    --object-id cc261686-88bf-4252-84c1-44b28dd1c533 `
    --secret-permissions get list
```

### App Pools Not Recycling?
```powershell
# Run in Windows PowerShell (NOT PowerShell 7):
C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe

# Then:
Import-Module WebAdministration
Restart-WebAppPool -Name "aidcircle-web"
Restart-WebAppPool -Name "aidcircle-api"
```

### Environment Variables Still Missing?
```powershell
# Check web.config manually:
notepad E:\aidcircle\web\web.config
notepad E:\aidcircle\api\web.config

# Look for this section:
<environmentVariables>
  <environmentVariable name="AZURE_CLIENT_ID" value="cc261686-88bf-4252-84c1-44b28dd1c533" />
  <environmentVariable name="KeyVaultName" value="aidcirclekeyvault" />
  ...
</environmentVariables>

# If missing, re-run Configure-IISArcIdentity.ps1
```

### Managed Identity Endpoint Not Responding?
```powershell
# 1. Check HIMDS service
Get-Service himds | Restart-Service

# 2. Check Arc agent
azcmagent show
# If not connected:
azcmagent connect --resource-group "rg-ss-vms" --tenant-id "YOUR_TENANT_ID" --subscription-id "YOUR_SUB_ID" --location "eastus"

# 3. Re-test endpoint WITH client_id parameter
$clientId = "cc261686-88bf-4252-84c1-44b28dd1c533"
$url = "http://localhost:40342/metadata/identity/oauth2/token?api-version=2020-06-01&resource=https://vault.azure.net&client_id=$clientId"
Invoke-RestMethod -Uri $url -Headers @{Metadata="true"} -Method GET
```

**Error: "Missing Basic Authorization header"**
- This means you forgot the `client_id` parameter
- Arc HIMDS requires `&client_id=YOUR_PRINCIPAL_ID` in the URL
- Without it, HIMDS doesn't know which identity to use

## ✅ Success Criteria

You'll know everything is working when:

1. ✅ `Get-Service himds` shows "Running"
2. ✅ `.\Verify-WebConfig.ps1` shows all green checkmarks
3. ✅ Managed identity endpoint returns JSON with `access_token`
4. ✅ Application logs show no DefaultAzureCredential errors
5. ✅ Web app loads at http://localhost:5011
6. ✅ API app responds at http://localhost:5135

## 📞 Still Stuck?

Check these logs:
```powershell
# Application logs
Get-Content "E:\aidcircle\web\logs\stdout_*.log" -Tail 100
Get-Content "E:\aidcircle\api\logs\stdout_*.log" -Tail 100

# Windows Event Log
Get-EventLog -LogName Application -Source "ASP.NET Core*" -Newest 50 | Format-Table -Wrap
```

Review documentation:
- `IIS-ENVIRONMENT-VARIABLES-GUIDE.md` - Environment variable setup
- `FIX-IIS-ARC-AUTHENTICATION-ERROR.md` - Authentication troubleshooting
- `CONFIGURE-IIS-ARC-SCRIPT-USAGE.md` - Script usage and parameters
- `FIXES-APPLIED-2025-11-15.md` - Detailed fix explanations
