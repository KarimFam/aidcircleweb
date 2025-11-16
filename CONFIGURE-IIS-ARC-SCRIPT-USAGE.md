# Configure-IISArcIdentity.ps1 - Usage Guide

## What This Script Does

Automates the configuration of Azure Arc managed identity authentication for IIS applications by:
1. ✅ Retrieving your Arc server's managed identity Principal ID from Azure
2. ✅ Verifying and granting Key Vault access permissions
3. ✅ Updating `web.config` files with application-specific environment variables
4. ✅ Recycling IIS application pools to apply changes

## Quick Start

### Interactive Mode (Recommended)

Run the script without parameters and it will prompt you for all required information:

```powershell
.\Configure-IISArcIdentity.ps1
```

You'll be asked for:
- **Arc Machine Name**: Your IIS server's hostname (e.g., `PROD-WEB-01`)
- **Arc Resource Group**: The Azure resource group where your Arc-enabled server is registered
- **Key Vault Name**: Your Azure Key Vault name (e.g., `aidcirclekeyvault`)
- **Azure OpenAI Endpoint**: Your Azure OpenAI endpoint URL
- **Web App Pool Name**: IIS application pool name for Web app (default: `AidCircleWebPool`)
- **API App Pool Name**: IIS application pool name for API app (default: `AidCircleApiPool`)

### Command-Line Mode

If you prefer to pass all parameters directly:

```powershell
.\Configure-IISArcIdentity.ps1 `
  -MachineName "WEB1" `
  -ArcResourceGroup "rg-ss-vms" `
  -ArcSubscription "KFAM1" `
  -KeyVaultName "aidcirclekeyvault" `
  -KeyVaultSubscription "DEVTEST" `
  -AzureOpenAIEndpoint "https://your-resource.openai.azure.com/" `
  -WebAppPath "E:\aidcircle\web" `
  -ApiAppPath "E:\aidcircle\api" `
  -WebAppPoolName "aidcircle-web" `
  -ApiAppPoolName "aidcircle-api"
```

**New Parameters for Multi-Subscription Support:**
- **ArcSubscription** (optional): Subscription where Arc machine is registered. If not specified, searches current subscription first, then all subscriptions.
- **KeyVaultSubscription** (optional): Subscription where Key Vault exists. If not specified, searches current subscription first, then all subscriptions.

### Verify Configuration

After running the configuration script, verify the environment variables were set correctly:

```powershell
.\Verify-WebConfig.ps1
```

This will check both web.config files for:
- ✅ AZURE_CLIENT_ID (managed identity)
- ✅ KeyVaultName
- ✅ Azure OpenAI settings
- ✅ Azure AD B2C settings (Web app only)

## Prerequisites

Before running this script:

1. ✅ **Azure Arc Agent Installed**: Run `azcmagent show` on your IIS server to verify
2. ✅ **HIMDS Service Running**: Run `Get-Service himds` (should show "Running")
3. ✅ **Azure CLI Installed**: Download from https://aka.ms/installazurecliwindows
4. ✅ **Logged into Azure**: Run `az login` before the script
5. ✅ **Applications Deployed**: Ensure `web.config` files exist in app directories
6. ✅ **Run as Administrator**: Script needs permissions to modify IIS and files

## Parameters Reference

| Parameter | Required | Default | Description |
|-----------|----------|---------|-------------|
| `MachineName` | No* | Prompts | Hostname of your Arc-enabled IIS server |
| `ArcResourceGroup` | No* | Prompts | Azure resource group where Arc machine is registered |
| `WebAppPath` | No | `E:\aidcircle\web` | Physical path to Web application |
| `ApiAppPath` | No | `E:\aidcircle\api` | Physical path to API application |
| `WebAppPoolName` | No* | Prompts (default: `AidCircleWebPool`) | IIS App Pool name for Web app |
| `ApiAppPoolName` | No* | Prompts (default: `AidCircleApiPool`) | IIS App Pool name for API app |
| `KeyVaultName` | No* | Prompts | Azure Key Vault name |
| `AzureOpenAIEndpoint` | No* | Prompts | Azure OpenAI endpoint URL |
| `AzureOpenAIDeployment` | No | `gpt-4o-mini` | Azure OpenAI deployment name |

*If not provided, the script will prompt interactively.

## Example Output

### Successful Run

```
========================================
IIS Azure Arc Identity Configuration
========================================

Enter the Azure Arc machine name: PROD-WEB-01
Enter the Azure Arc resource group name: rg-aidcircle-arc
Enter the Key Vault name: aidcirclekeyvault
Enter the Azure OpenAI endpoint: https://my-openai.openai.azure.com/

Configuration Summary:
  Arc Machine Name: PROD-WEB-01
  Arc Resource Group: rg-aidcircle-arc
  Key Vault: aidcirclekeyvault
  Web App Path: E:\aidcircle\web
  API App Path: E:\aidcircle\api
  Web App Pool: AidCircleWebPool
  API App Pool: AidCircleApiPool

Proceed with configuration? (Y/N): Y

[1/7] Checking Azure CLI...
✅ Azure CLI version 2.79.0 found
[2/7] Checking Azure login...
✅ Logged in as: KFAM1
[3/7] Retrieving Arc Managed Identity...
✅ Managed Identity Principal ID: cc261686-88bf-4252-84c1-44b28dd1c533
[4/7] Granting Key Vault access...
✅ Key Vault access granted
[5/7] Updating Web app web.config...
✅ Web app web.config updated (backup: E:\aidcircle\web\web.config.backup_20251110_143052)
[6/7] Updating API app web.config...
✅ API app web.config updated (backup: E:\aidcircle\api\web.config.backup_20251110_143052)
[7/7] Recycling application pools...
✅ Recycled AidCircleWebPool
✅ Recycled AidCircleApiPool

========================================
Configuration Complete!
========================================
```

## Troubleshooting

### Error: "Could not retrieve managed identity"

**Cause**: Arc agent not installed or not connected to Azure

**Solution**:
```powershell
# Verify Arc agent status
azcmagent show

# If not connected, check HIMDS service
Get-Service himds
Start-Service himds
```

### Error: "Key Vault not found"

**Cause**: Key Vault name is incorrect or doesn't exist in current subscription

**Solution**: The script will list available Key Vaults and prompt you to enter the correct name.

### Error: "Could not find aspNetCore element in web.config"

**Cause**: `web.config` has non-standard structure

**Solution**: Manually edit `web.config` following the guide: `IIS-ENVIRONMENT-VARIABLES-GUIDE.md`

### Error: "App pool not found"

**Cause**: App pool name doesn't match what's in IIS

**Solution**: The script will list available app pools. Re-run with correct names:
```powershell
# List all app pools
Get-ChildItem IIS:\AppPools | Select-Object Name
```

## What Gets Modified

### web.config Files

The script adds/updates the `<environmentVariables>` section in both:
- `E:\aidcircle\web\web.config`
- `E:\aidcircle\api\web.config`

**Example** (before and after):

**BEFORE**:
```xml
<aspNetCore processPath="dotnet"
            arguments=".\H4H.Presentation.Web.dll"
            stdoutLogEnabled="true"
            stdoutLogFile=".\logs\stdout"
            hostingModel="inprocess">
</aspNetCore>
```

**AFTER**:
```xml
<aspNetCore processPath="dotnet"
            arguments=".\H4H.Presentation.Web.dll"
            stdoutLogEnabled="true"
            stdoutLogFile=".\logs\stdout"
            hostingModel="inprocess">
  <environmentVariables>
    <environmentVariable name="AZURE_CLIENT_ID" value="cc261686-88bf-4252-84c1-44b28dd1c533" />
    <environmentVariable name="KeyVaultName" value="aidcirclekeyvault" />
    <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
    <!-- ... and more ... -->
  </environmentVariables>
</aspNetCore>
```

**Backups**: Original files are backed up before modification:
- `web.config.backup_YYYYMMDD_HHMMSS`

### Key Vault Access Policy

The script grants the Arc managed identity these permissions:
- **Secret permissions**: `get`, `list`

You can verify this in Azure Portal:
- Key Vault → Access policies → Look for your Arc machine's principal ID

## Security Notes

✅ **Application-Specific Variables**: Environment variables are set in `web.config` (not machine-level), ensuring isolation per application.

✅ **No Plaintext Secrets**: API keys and database passwords are loaded from Key Vault at runtime, not stored in `web.config`.

✅ **Managed Identity**: Uses Azure Arc's managed identity for passwordless authentication to Azure services.

✅ **Backups**: All `web.config` files are backed up before modification.

## Next Steps After Running Script

1. **Verify Configuration**:
   ```powershell
   .\Verify-WebConfig.ps1
   # Checks that AZURE_CLIENT_ID and other variables are correctly set
   ```

2. **Verify HIMDS Service**:
   ```powershell
   Get-Service himds  # Should be "Running"
   ```

3. **Test Managed Identity Endpoint**:
   ```powershell
   # CRITICAL: Include client_id parameter (your Arc Principal ID)
   $clientId = "cc261686-88bf-4252-84c1-44b28dd1c533"  # Your Principal ID
   $url = "http://localhost:40342/metadata/identity/oauth2/token?api-version=2020-06-01&resource=https://vault.azure.net&client_id=$clientId"
   Invoke-RestMethod -Uri $url -Headers @{Metadata="true"} -Method GET
   # Should return JSON with access_token
   ```
   
   **Common Error:**
   ```json
   { "error": "unauthorized_client", "error_description": "Missing Basic Authorization header" }
   ```
   **Fix:** Add `&client_id=YOUR_PRINCIPAL_ID` to the URL

4. **Recycle App Pools** (if not done automatically):
   ```powershell
   # In Windows PowerShell (not PowerShell 7)
   Import-Module WebAdministration
   Restart-WebAppPool -Name "aidcircle-web"
   Restart-WebAppPool -Name "aidcircle-api"
   ```

5. **Check Application Logs**:
   ```powershell
   Get-Content "E:\aidcircle\web\logs\stdout_*.log" -Tail 50
   # Look for: "Hosting environment: Production"
   # Should NOT see: "CredentialUnavailableException"
   # Should NOT see: "EnvironmentCredential authentication unavailable"
   ```

6. **Browse Application**:
   - Open browser to: `http://localhost:5011` (Web app)
   - Open browser to: `http://localhost:5135` (API app)
   - Application should load without authentication errors

## Troubleshooting Common Issues

### Issue: "DefaultAzureCredential failed to retrieve a token"

**Symptoms**:
```
EnvironmentCredential authentication unavailable. Environment variables are not fully configured.
ManagedIdentityCredential authentication unavailable. No response received from the managed identity endpoint.
```

**Causes**:
1. ❌ AZURE_CLIENT_ID not set in web.config
2. ❌ App pool not recycled after web.config changes
3. ❌ HIMDS service not running
4. ❌ Arc agent not connected

**Solutions**:
```powershell
# 1. Verify web.config has AZURE_CLIENT_ID
.\Verify-WebConfig.ps1

# 2. Manually recycle app pools (Windows PowerShell)
Import-Module WebAdministration
Restart-WebAppPool -Name "aidcircle-web"
Restart-WebAppPool -Name "aidcircle-api"

# 3. Check HIMDS service
Get-Service himds  # Should be "Running"
# If stopped:
Start-Service himds

# 4. Check Arc agent
azcmagent show
# If not connected:
azcmagent connect --resource-group "rg-ss-vms" --tenant-id "YOUR_TENANT_ID" --subscription-id "YOUR_SUB_ID" --location "eastus"
```

### Issue: "Could not grant Key Vault access"

**Symptoms**:
```
⚠️  Could not grant Key Vault access. You may need to do this manually
```

**Causes**:
1. ❌ Insufficient permissions in Key Vault subscription
2. ❌ Key Vault in different subscription than current context
3. ❌ RBAC permission issue

**Solutions**:
```powershell
# Manual access grant:
az login
az account set --subscription "DEVTEST"  # Key Vault subscription
az keyvault set-policy --name aidcirclekeyvault --object-id cc261686-88bf-4252-84c1-44b28dd1c533 --secret-permissions get list

# Verify access:
az keyvault secret show --vault-name aidcirclekeyvault --name "some-secret" --query id
```

### Issue: App pools not found or not recycled

**Symptoms**:
```
⚠️  App pool 'aidcircle-web' not found
Cannot find drive. A drive with the name 'IIS' does not exist.
```

**Causes**:
1. ❌ Running in PowerShell 7 (WebAdministration module compatibility issue)
2. ❌ App pool name incorrect

**Solutions**:
```powershell
# Use Windows PowerShell for IIS management:
C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe

# List available app pools:
Import-Module WebAdministration
Get-ChildItem IIS:\AppPools | Select-Object Name

# Recycle with correct name:
Restart-WebAppPool -Name "aidcircle-web"
```

### Issue: Arc machine not found

**Symptoms**:
```
❌ Could not retrieve managed identity. Arc machine not found.
```

**Causes**:
1. ❌ Machine name incorrect
2. ❌ Resource group incorrect
3. ❌ Arc machine in different subscription
4. ❌ Arc agent not connected

**Solutions**:
```powershell
# Check Arc agent status on IIS server:
azcmagent show

# Search for Arc machine across all subscriptions:
az login
$allSubs = az account list --query "[].name" -o tsv
foreach ($sub in $allSubs) {
    Write-Host "Checking subscription: $sub"
    az account set --subscription $sub
    az connectedmachine list -o table
}

# Re-run script with correct subscription:
.\Configure-IISArcIdentity.ps1 -ArcSubscription "KFAM1" -MachineName "WEB1" -ArcResourceGroup "rg-ss-vms"
```

## Manual Fallback

If the script fails to update `web.config` files, follow the manual guide:
- **Guide**: `IIS-ENVIRONMENT-VARIABLES-GUIDE.md`
- **Section**: "Step-by-Step Configuration"

## Related Documentation

- **Troubleshooting Guide**: `FIX-IIS-ARC-AUTHENTICATION-ERROR.md`
- **Full IIS Deployment**: `docs/deployment-iis-arc.md`
- **Environment Variables Guide**: `IIS-ENVIRONMENT-VARIABLES-GUIDE.md`
- **Secrets Management**: `SECRETS-MANAGEMENT-IMPLEMENTATION.md`

## Support

If you encounter issues:
1. Check the script output for specific error messages
2. Review the troubleshooting section above
3. Consult `FIX-IIS-ARC-AUTHENTICATION-ERROR.md` for your specific error
4. Verify all prerequisites are met
