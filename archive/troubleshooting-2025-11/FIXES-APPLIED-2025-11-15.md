# Fixes Applied - November 15, 2025

## Issues Fixed in Configure-IISArcIdentity.ps1

### 1. ✅ Multi-Subscription Support for Arc Machine
**Problem**: Script only searched current subscription for Arc machine, failing when machine was in different subscription (KFAM1 vs DEVTEST).

**Fix**:
- Added `$ArcSubscription` parameter
- If specified, switches to that subscription first
- If not specified, tries current subscription, then searches ALL subscriptions
- Stores Arc machine subscription context for later restoration

**Impact**: Script now finds Arc machine `WEB1` in subscription `KFAM1` even when running in `DEVTEST` subscription.

---

### 2. ✅ Key Vault Access Grant Failure
**Problem**: Script switched to Key Vault subscription (`DEVTEST`) but didn't properly manage subscription context, causing access grant to fail.

**Error**:
```
⚠️  Could not grant Key Vault access. You may need to do this manually
```

**Fix**:
- Store Arc machine subscription ID before searching for Key Vault
- Switch to Key Vault subscription for `az keyvault set-policy`
- **Always restore to Arc machine subscription** after Key Vault operations
- Better error handling with actual subscription ID in manual instructions

**Impact**: `az keyvault set-policy` now runs in correct subscription context (`DEVTEST`), successfully granting access.

---

### 3. ✅ PowerShell 7 Compatibility for IIS Management
**Problem**: `WebAdministration` module doesn't work properly in PowerShell 7, causing IIS app pool operations to fail.

**Errors**:
```
Cannot find drive. A drive with the name 'IIS' does not exist.
WARNING: Module WebAdministration is loaded in Windows PowerShell using WinPSCompatSession remoting session
```

**Fix**:
- Detect Windows PowerShell path: `$env:SystemRoot\System32\WindowsPowerShell\v1.0\powershell.exe`
- Execute IIS commands in Windows PowerShell 5.1 using `& $psPath -Command`
- Return success/failure status from Windows PowerShell execution
- Provide fallback manual instructions if automation fails

**Impact**: App pools now recycle successfully even when script runs in PowerShell 7.

---

### 4. ✅ Environment Variables Verification
**Problem**: No easy way to verify if `web.config` files were correctly updated with `AZURE_CLIENT_ID` and other critical variables.

**Fix**: Created `Verify-WebConfig.ps1` script to:
- Parse both web.config files (Web and API)
- Check for presence and values of critical environment variables
- Display ✅/❌ status for each variable
- Provide clear next steps if variables are missing

**Critical Variables Checked**:
- `AZURE_CLIENT_ID` (managed identity Principal ID)
- `KeyVaultName`
- `ASPNETCORE_ENVIRONMENT`
- `AzureOpenAI__Endpoint`
- `AzureAdB2C__Instance` (Web app only)
- `ApiSettings__BaseUrl` (Web app only)

---

## Root Cause: DefaultAzureCredential Failure

### Original Error
```
DefaultAzureCredential failed to retrieve a token from the included credentials.
- EnvironmentCredential authentication unavailable. Environment variables are not fully configured.
- ManagedIdentityCredential authentication unavailable. No response received from the managed identity endpoint.
```

### Why This Happened
1. **Missing AZURE_CLIENT_ID**: Even though `web.config` was updated, app pool wasn't recycled
2. **HIMDS Endpoint**: Managed identity endpoint (`localhost:40342`) requires `AZURE_CLIENT_ID` environment variable to know which identity to use
3. **App Pool Cache**: IIS caches environment variables in app pool worker process; changes require recycle

### How It's Fixed Now
1. ✅ `AZURE_CLIENT_ID` correctly set in `web.config` with Principal ID: `cc261686-88bf-4252-84c1-44b28dd1c533`
2. ✅ App pools automatically recycled using Windows PowerShell
3. ✅ Verification script confirms environment variables are present
4. ✅ Multi-subscription support ensures Arc identity is retrieved from correct subscription

---

## New Workflow

### 1. Run Configuration Script
```powershell
.\Configure-IISArcIdentity.ps1 `
    -MachineName "WEB1" `
    -ArcResourceGroup "rg-ss-vms" `
    -ArcSubscription "KFAM1" `
    -KeyVaultName "aidcirclekeyvault" `
    -KeyVaultSubscription "DEVTEST" `
    -WebAppPoolName "aidcircle-web" `
    -ApiAppPoolName "aidcircle-api"
```

**Expected Output**:
```
[3/7] Retrieving Arc Managed Identity...
   Switching to Arc machine subscription: KFAM1
   ✅ Found Arc machine in specified subscription
✅ Managed Identity Principal ID: cc261686-88bf-4252-84c1-44b28dd1c533

[4/7] Granting Key Vault access...
   Arc machine subscription: KFAM1
   Switching to Key Vault subscription: DEVTEST
   ✅ Key Vault found in current subscription context
✅ Key Vault access granted

[5/7] Updating Web app web.config...
✅ Web app web.config updated

[6/7] Updating API app web.config...
✅ API app web.config updated

[7/7] Recycling application pools...
✅ Recycled aidcircle-web
✅ Recycled aidcircle-api
```

### 2. Verify Configuration
```powershell
.\Verify-WebConfig.ps1
```

**Expected Output**:
```
Checking Web Application...
✅ [WEB] AZURE_CLIENT_ID = cc261686-88bf-4252-84c1-44b28dd1c533
✅ [WEB] KeyVaultName = aidcirclekeyvault
✅ [WEB] ASPNETCORE_ENVIRONMENT = Production
...

✅ Web app configuration looks good!
✅ API app configuration looks good!
```

### 3. Test Managed Identity
```powershell
$url = "http://localhost:40342/metadata/identity/oauth2/token?api-version=2020-06-01&resource=https://vault.azure.net"
Invoke-RestMethod -Uri $url -Headers @{Metadata="true"} -Method GET
```

**Expected Output**: JSON with `access_token`, `expires_on`, `token_type`

### 4. Check Application
```powershell
# View logs
Get-Content "E:\aidcircle\web\logs\stdout_*.log" -Tail 50

# Should NOT see:
# - "CredentialUnavailableException"
# - "EnvironmentCredential authentication unavailable"

# Should see:
# - "Hosting environment: Production"
# - "Now listening on: http://localhost:5011"
```

---

## Files Modified

### Scripts
- ✅ `Configure-IISArcIdentity.ps1` - Multi-subscription support, better error handling, PowerShell 7 compatibility
- ✅ `Verify-WebConfig.ps1` - **NEW** - Environment variable verification

### Documentation
- ✅ `CONFIGURE-IIS-ARC-SCRIPT-USAGE.md` - Updated with new parameters, verification steps, troubleshooting
- ✅ `FIXES-APPLIED-2025-11-15.md` - **NEW** - This document

---

## Testing Checklist

After applying fixes, verify:

- [ ] Arc machine found in correct subscription (KFAM1)
- [ ] Key Vault access granted successfully (subscription: DEVTEST)
- [ ] web.config files updated with AZURE_CLIENT_ID
- [ ] App pools recycled (aidcircle-web, aidcircle-api)
- [ ] Managed identity endpoint returns access token
- [ ] Web application starts without DefaultAzureCredential errors
- [ ] API application starts without DefaultAzureCredential errors
- [ ] Application can access secrets from Key Vault

---

## Manual Fallback (If Automation Fails)

### Manually Grant Key Vault Access
```powershell
az login
az account set --subscription "DEVTEST"
az keyvault set-policy `
    --name aidcirclekeyvault `
    --object-id cc261686-88bf-4252-84c1-44b28dd1c533 `
    --secret-permissions get list
```

### Manually Recycle App Pools (Windows PowerShell)
```powershell
Import-Module WebAdministration
Restart-WebAppPool -Name "aidcircle-web"
Restart-WebAppPool -Name "aidcircle-api"
```

### Manually Edit web.config
If `Verify-WebConfig.ps1` shows missing variables, edit `E:\aidcircle\web\web.config`:

```xml
<environmentVariables>
  <environmentVariable name="AZURE_CLIENT_ID" value="cc261686-88bf-4252-84c1-44b28dd1c533" />
  <environmentVariable name="KeyVaultName" value="aidcirclekeyvault" />
  <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
  <!-- Add other variables as needed -->
</environmentVariables>
```

Then recycle app pool manually.

---

## Key Takeaways

1. **Multi-Subscription is Common**: Arc machines and Key Vaults are often in different subscriptions for organizational/security reasons
2. **Environment Variables**: `AZURE_CLIENT_ID` is **REQUIRED** for DefaultAzureCredential to use managed identity
3. **App Pool Recycling**: Critical step - environment variable changes don't take effect until app pool restarts
4. **PowerShell Versions**: IIS management requires Windows PowerShell 5.1, not PowerShell 7
5. **Verification**: Always verify configuration with `Verify-WebConfig.ps1` before testing application

---

## Next Steps for Production

1. Run `Configure-IISArcIdentity.ps1` on production IIS servers
2. Verify configuration with `Verify-WebConfig.ps1`
3. Test managed identity endpoint
4. Deploy applications
5. Monitor logs for any authentication issues
6. Set up SSL certificates for production URLs (api.aidcircle.net, aidcircle.net)
