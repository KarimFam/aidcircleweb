# IIS Application-Specific Environment Variables Guide

## Quick Reference: Why Use web.config Instead of Machine-Level Variables

### ❌ WRONG: Machine-Level Variables (Security Risk)

```powershell
# DON'T DO THIS - Makes secrets available to ALL apps on server
[Environment]::SetEnvironmentVariable("KeyVaultName", "kv-aidcircle-prod", "Machine")
[Environment]::SetEnvironmentVariable("ApiSettings__ApiKey", "secret-key", "Machine")
```

**Problems**:
- 🚨 All IIS applications can read these secrets
- 🚨 Other users on the server can access them
- 🚨 Difficult to have different configs per app
- 🚨 Requires machine restart or IIS restart to apply

### ✅ CORRECT: Application-Specific via web.config

Each app has its own isolated environment variables in `web.config`.

---

## Step-by-Step Configuration

### Step 1: Locate Your web.config File

For AidCircle, you'll have two:
- **API**: `E:\aidcircle\api\web.config` (or `C:\inetpub\aidcircle\api\web.config`)
- **Web**: `E:\aidcircle\web\web.config` (or `C:\inetpub\aidcircle\web\web.config`)

### Step 2: Edit web.config to Add Environment Variables

Open `web.config` in a text editor (run as Administrator):

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet"
                  arguments=".\H4H.Presentation.Web.dll"
                  stdoutLogEnabled="true"
                  stdoutLogFile=".\logs\stdout"
                  hostingModel="inprocess">
        
        <!-- ✅ APPLICATION-SPECIFIC ENVIRONMENT VARIABLES -->
        <environmentVariables>
          <!-- Environment -->
          <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
          
          <!-- Azure Arc Managed Identity (CRITICAL for authentication) -->
          <environmentVariable name="AZURE_CLIENT_ID" value="PASTE_YOUR_MANAGED_IDENTITY_PRINCIPAL_ID_HERE" />
          
          <!-- Key Vault Name -->
          <environmentVariable name="KeyVaultName" value="kv-aidcircle-prod" />
          
          <!-- Azure AD B2C (for Web app only) -->
          <environmentVariable name="AzureAdB2C__Instance" value="https://aidcirclenet.b2clogin.com/" />
          <environmentVariable name="AzureAdB2C__Domain" value="aidcirclenet.onmicrosoft.com" />
          <environmentVariable name="AzureAdB2C__ClientId" value="36bf28fa-d15c-4762-8c32-8e46c3aa9051" />
          <environmentVariable name="AzureAdB2C__SignUpSignInPolicyId" value="B2C_1_susi" />
          
          <!-- Azure OpenAI -->
          <environmentVariable name="AzureOpenAI__Endpoint" value="https://YOUR-RESOURCE.openai.azure.com/" />
          <environmentVariable name="AzureOpenAI__DeploymentName" value="gpt-4o-mini" />
          
          <!-- Azure Translator -->
          <environmentVariable name="AzureTranslator__Endpoint" value="https://api.cognitive.microsofttranslator.com" />
          <environmentVariable name="AzureTranslator__Region" value="eastus" />
          
          <!-- API Client (for Web app only) -->
          <environmentVariable name="ApiSettings__BaseUrl" value="http://localhost:5135" />
          <!-- ApiSettings__ApiKey loaded from Key Vault -->
          
          <!-- Database Connection (optional - prefer Key Vault) -->
          <!-- <environmentVariable name="ConnectionStrings__H4HDB-DEV" value="Server=..." /> -->
        </environmentVariables>
        
      </aspNetCore>
    </system.webServer>
  </location>
</configuration>
```

### Step 3: Get Your Azure Arc Managed Identity Principal ID

Run this PowerShell command **on the IIS server**:

```powershell
# Replace with your actual values
$machineName = "YOUR-IIS-SERVER-HOSTNAME"  # e.g., "PROD-WEB-01"
$resourceGroup = "rg-aidcircle-arc"        # Your Azure resource group

# Get the Principal ID
$principalId = az connectedmachine show `
  --name $machineName `
  --resource-group $resourceGroup `
  --query "identity.principalId" -o tsv

Write-Host "Your AZURE_CLIENT_ID: $principalId" -ForegroundColor Green
Write-Host "Copy this value into web.config" -ForegroundColor Yellow
```

**Alternative: Use Azure Portal**
1. Go to Azure Portal → Search for "Azure Arc" → "Servers"
2. Find your server → Click on it
3. Go to **Identity** blade
4. Copy the **Object (principal) ID**

### Step 4: Copy the Principal ID to web.config

Replace `PASTE_YOUR_MANAGED_IDENTITY_PRINCIPAL_ID_HERE` with the actual value:

```xml
<environmentVariable name="AZURE_CLIENT_ID" value="a1b2c3d4-e5f6-7890-abcd-ef1234567890" />
```

### Step 5: Recycle Application Pool (Apply Changes)

```powershell
# No need to restart entire IIS - just recycle the app pool
Restart-WebAppPool -Name "AidCircleWebPool"
Restart-WebAppPool -Name "AidCircleApiPool"

# Verify they restarted
Get-WebAppPoolState -Name "AidCircleWebPool"
Get-WebAppPoolState -Name "AidCircleApiPool"
# Should show: Started
```

**Why recycle instead of IIS restart?**
- ✅ Faster (seconds vs minutes)
- ✅ Doesn't affect other apps on the server
- ✅ No downtime for other websites

### Step 6: Verify Environment Variables are Loaded

Check the application startup logs:

```powershell
# View latest stdout log
Get-Content "E:\aidcircle\web\logs\stdout_*.log" -Tail 50

# Look for lines like:
# info: Microsoft.Hosting.Lifetime[0]
#       Hosting environment: Production
```

---

## Troubleshooting

### Problem: Variables Not Loading

**Symptom**: App crashes with "configuration value is null"

**Solution**:
1. Check XML syntax is valid (no typos, proper closing tags)
2. Verify `<environmentVariables>` is INSIDE `<aspNetCore>` tag
3. Recycle app pool after changes

### Problem: Still Getting DefaultAzureCredential Error

**Error**:
```
EnvironmentCredential authentication unavailable. Environment variables are not fully configured.
```

**Solution**:
1. Ensure `AZURE_CLIENT_ID` is set in `web.config`
2. Verify HIMDS service is running: `Get-Service himds`
3. Test Arc agent: `azcmagent show` (should show "Connected")
4. Check stdout logs for detailed error

### Problem: Key Vault Access Denied

**Error**:
```
Azure.RequestFailedException: The user, group or application '...' does not have secrets get permission
```

**Solution**:
```powershell
# Grant Key Vault access to Arc managed identity
$vaultName = "kv-aidcircle-prod"
$principalId = "YOUR_MANAGED_IDENTITY_PRINCIPAL_ID"

az keyvault set-policy `
  --name $vaultName `
  --object-id $principalId `
  --secret-permissions get list
```

---

## Comparison: Machine vs Application-Level Variables

| Aspect | Machine-Level | Application-Level (web.config) |
|--------|---------------|--------------------------------|
| **Scope** | All apps on server | Single app only |
| **Security** | ❌ Low (all users can access) | ✅ High (isolated per app) |
| **Configuration** | PowerShell commands | Edit XML file |
| **Apply Changes** | Restart IIS or reboot | Recycle app pool |
| **Downtime** | Minutes | Seconds |
| **Different Values per App** | ❌ No | ✅ Yes |
| **Recommended For** | System-wide settings | **Application secrets** |

---

## Example: Full web.config for Web App

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet"
                  arguments=".\H4H.Presentation.Web.dll"
                  stdoutLogEnabled="true"
                  stdoutLogFile=".\logs\stdout"
                  hostingModel="inprocess">
        <environmentVariables>
          <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
          <environmentVariable name="AZURE_CLIENT_ID" value="a1b2c3d4-e5f6-7890-abcd-ef1234567890" />
          <environmentVariable name="KeyVaultName" value="kv-aidcircle-prod" />
          <environmentVariable name="AzureAdB2C__Instance" value="https://aidcirclenet.b2clogin.com/" />
          <environmentVariable name="AzureAdB2C__Domain" value="aidcirclenet.onmicrosoft.com" />
          <environmentVariable name="AzureAdB2C__ClientId" value="36bf28fa-d15c-4762-8c32-8e46c3aa9051" />
          <environmentVariable name="AzureAdB2C__SignUpSignInPolicyId" value="B2C_1_susi" />
          <environmentVariable name="AzureOpenAI__Endpoint" value="https://YOUR-RESOURCE.openai.azure.com/" />
          <environmentVariable name="AzureOpenAI__DeploymentName" value="gpt-4o-mini" />
          <environmentVariable name="AzureTranslator__Endpoint" value="https://api.cognitive.microsofttranslator.com" />
          <environmentVariable name="AzureTranslator__Region" value="eastus" />
          <environmentVariable name="ApiSettings__BaseUrl" value="http://localhost:5135" />
        </environmentVariables>
      </aspNetCore>
    </system.webServer>
  </location>
</configuration>
```

## Example: Full web.config for API App

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet"
                  arguments=".\H4H.Presentation.API.dll"
                  stdoutLogEnabled="true"
                  stdoutLogFile=".\logs\stdout"
                  hostingModel="inprocess">
        <environmentVariables>
          <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
          <environmentVariable name="AZURE_CLIENT_ID" value="a1b2c3d4-e5f6-7890-abcd-ef1234567890" />
          <environmentVariable name="KeyVaultName" value="kv-aidcircle-prod" />
          <environmentVariable name="AzureOpenAI__Endpoint" value="https://YOUR-RESOURCE.openai.azure.com/" />
          <environmentVariable name="AzureOpenAI__DeploymentName" value="gpt-4o-mini" />
          <environmentVariable name="AzureTranslator__Endpoint" value="https://api.cognitive.microsofttranslator.com" />
          <environmentVariable name="AzureTranslator__Region" value="eastus" />
          <environmentVariable name="ApiSettings__AllowedOrigins__0" value="https://aidcircle.net" />
          <environmentVariable name="ApiSettings__AllowedOrigins__1" value="http://localhost:5011" />
        </environmentVariables>
      </aspNetCore>
    </system.webServer>
  </location>
</configuration>
```

---

## Quick Checklist

Before deploying to IIS with Azure Arc:

- [ ] Azure Arc agent installed and connected (`azcmagent show`)
- [ ] HIMDS service running (`Get-Service himds`)
- [ ] Managed identity enabled on Arc server
- [ ] Managed identity has Key Vault access permissions
- [ ] `AZURE_CLIENT_ID` environment variable set in `web.config`
- [ ] `KeyVaultName` environment variable set in `web.config`
- [ ] All other required config values in `web.config`
- [ ] App pools recycled after `web.config` changes
- [ ] Stdout logging enabled (`stdoutLogEnabled="true"`)
- [ ] Logs directory exists and is writable

---

## Next Steps

- 📚 **Full IIS Deployment Guide** → [deployment-iis-arc.md](docs/deployment-iis-arc.md)
- 🔐 **Secrets Management** → [SECRETS-MANAGEMENT-IMPLEMENTATION.md](SECRETS-MANAGEMENT-IMPLEMENTATION.md)
- 🔍 **Troubleshooting** → See [deployment-iis-arc.md#troubleshooting](docs/deployment-iis-arc.md#troubleshooting)
