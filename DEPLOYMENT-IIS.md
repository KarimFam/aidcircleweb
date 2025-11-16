# Deploying AidCircle to IIS with Azure Key Vault

## Overview

This guide covers deploying the AidCircle ASP.NET Core application to IIS with secure Azure Key Vault integration using **Azure AD App Registration** (Client ID + Secret).

**⚠️ CRITICAL LESSON LEARNED**: Azure Arc Managed Identity with IIS has proven problematic due to DefaultAzureCredential's challenge-response mechanism not working reliably in IIS worker processes. **Use App Registration with Client Secret instead** - it's simpler, more reliable, and well-tested.

## Prerequisites

- Windows Server with IIS installed
- .NET 8.0 Hosting Bundle for IIS
- Azure subscription with Key Vault access
- Azure CLI installed on deployment machine

## Architecture

```
IIS Application Pool (w3wp.exe)
  ↓ reads environment variables from web.config
  ↓ AZURE_TENANT_ID, AZURE_CLIENT_ID, AZURE_CLIENT_SECRET
  ↓
DefaultAzureCredential (Azure.Identity)
  ↓ uses EnvironmentCredential (client secret flow)
  ↓
Azure AD Token Endpoint
  ↓ returns access token
  ↓
Azure Key Vault
  ↓ retrieves secrets
  ↓
Application (loads configuration)
```

## Step 1: Create Azure AD App Registration

Run this script to create app registration and generate secrets:

```powershell
.\Setup-AppRegistration.ps1
```

**Manual steps if needed**:

```powershell
# Create app registration
$appId = az ad app create --display-name "aidcircle-app" --query "appId" -o tsv

# Create service principal
az ad sp create --id $appId

# Generate client secret
$secret = az ad app credential reset --id $appId --query "password" -o tsv

# Get tenant ID
$tenantId = az account show --query tenantId -o tsv

# Save these values securely
Write-Host "Tenant ID: $tenantId"
Write-Host "Client ID: $appId"
Write-Host "Client Secret: $secret"
```

## Step 2: Grant Key Vault Access

```powershell
# Get service principal object ID
$spObjectId = az ad sp show --id <CLIENT_ID> --query "id" -o tsv

# Grant Key Vault Secrets User role (RBAC)
az role assignment create `
  --role "Key Vault Secrets User" `
  --assignee $spObjectId `
  --scope "/subscriptions/<SUBSCRIPTION_ID>/resourceGroups/<RESOURCE_GROUP>/providers/Microsoft.KeyVault/vaults/<KEYVAULT_NAME>"
```

**Note**: If your Key Vault uses access policies instead of RBAC:

```powershell
az keyvault set-policy `
  --name <KEYVAULT_NAME> `
  --object-id $spObjectId `
  --secret-permissions get list
```

## Step 3: Configure IIS Application

### Update web.config

Add environment variables to **both** `web.config` files (Web and API):

```xml
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <aspNetCore processPath="dotnet" 
                  arguments=".\YourApp.dll" 
                  stdoutLogEnabled="true" 
                  stdoutLogFile=".\logs\stdout" 
                  hostingModel="inprocess">
        <environmentVariables>
          <environmentVariable name="AZURE_TENANT_ID" value="YOUR_TENANT_ID" />
          <environmentVariable name="AZURE_CLIENT_ID" value="YOUR_CLIENT_ID" />
          <environmentVariable name="AZURE_CLIENT_SECRET" value="YOUR_CLIENT_SECRET" />
          <environmentVariable name="KeyVaultName" value="YOUR_KEYVAULT_NAME" />
          <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
        </environmentVariables>
      </aspNetCore>
    </system.webServer>
  </location>
</configuration>
```

**⚠️ SECURITY NOTE**: 
- web.config files should have restricted NTFS permissions (IIS AppPool identity + Administrators only)
- Consider using Azure Key Vault references in web.config for production (future enhancement)
- Never commit web.config with actual secrets to source control

### Application Code Requirements

Your `Program.cs` must configure DefaultAzureCredential correctly:

```csharp
var keyVaultName = builder.Configuration["KeyVaultName"];
if (!string.IsNullOrEmpty(keyVaultName))
{
    var keyVaultUri = new Uri($"https://{keyVaultName}.vault.azure.net/");
    
    var credentialOptions = new DefaultAzureCredentialOptions
    {
        // Exclude credential types that don't work in IIS
        ExcludeVisualStudioCredential = true,
        ExcludeVisualStudioCodeCredential = true,
        ExcludeAzureCliCredential = true,
        ExcludeAzurePowerShellCredential = true,
        ExcludeSharedTokenCacheCredential = true,
        ExcludeInteractiveBrowserCredential = true,
        
        // Set managed identity client ID (will be ignored if AZURE_CLIENT_SECRET is present)
        ManagedIdentityClientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID")
    };
    
    builder.Configuration.AddAzureKeyVault(
        keyVaultUri,
        new DefaultAzureCredential(credentialOptions)
    );
}
```

## Step 4: Deploy Application

### Option A: Visual Studio Publish

1. Right-click project → Publish
2. Choose target: Folder or IIS server
3. Build configuration: Release
4. Deploy to target location (e.g., `E:\aidcircle\web`)

### Option B: Command Line

```powershell
# Publish Web app
dotnet publish H4H.Presentation.Web/H4H.Presentation.Web/H4H.Presentation.Web.csproj `
  --configuration Release `
  --output "C:\inetpub\wwwroot\aidcircle-web"

# Publish API
dotnet publish H4H.Presentation.API/H4H.Presentation.API.csproj `
  --configuration Release `
  --output "C:\inetpub\wwwroot\aidcircle-api"
```

### Configure IIS Sites

```powershell
# Import IIS module (Windows PowerShell 5.1)
Import-Module WebAdministration

# Create Web App Pool
New-WebAppPool -Name "aidcircle-web"
Set-ItemProperty IIS:\AppPools\aidcircle-web -Name managedRuntimeVersion -Value ""

# Create Web Site
New-Website -Name "aidcircle-web" `
  -ApplicationPool "aidcircle-web" `
  -PhysicalPath "C:\inetpub\wwwroot\aidcircle-web" `
  -Port 5011

# Repeat for API
New-WebAppPool -Name "aidcircle-api"
Set-ItemProperty IIS:\AppPools\aidcircle-api -Name managedRuntimeVersion -Value ""

New-Website -Name "aidcircle-api" `
  -ApplicationPool "aidcircle-api" `
  -PhysicalPath "C:\inetpub\wwwroot\aidcircle-api" `
  -Port 5135
```

## Step 5: Verify Deployment

### Check Application Logs

```powershell
# View latest logs
Get-Content "C:\inetpub\wwwroot\aidcircle-web\logs\stdout_*.log" -Tail 50
```

**Expected output on success**:
```
[KeyVault] Using Key Vault: your-keyvault-name
[KeyVault] Environment: Production
[KeyVault] AZURE_CLIENT_ID from environment: <your-client-id>
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5011
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

**Common errors**:

| Error | Cause | Solution |
|-------|-------|----------|
| `Azure.Identity.CredentialUnavailableException` | Environment variables not set or app pool not recycled | Update web.config, recycle app pool |
| `403 Forbidden from Key Vault` | Service principal lacks RBAC role | Grant "Key Vault Secrets User" role |
| `Connection string not found` | Key Vault secret missing | Add secret to Key Vault: `az keyvault secret set --vault-name <name> --name "H4HDB-DEV" --value "<connection-string>"` |

### Test Application

```powershell
# Test Web app
Invoke-WebRequest -Uri "http://localhost:5011" -UseBasicParsing

# Test API
Invoke-WebRequest -Uri "http://localhost:5135/swagger" -UseBasicParsing
```

## Troubleshooting

### App Pool Won't Start

1. Check Event Viewer: Windows Logs → Application
2. Look for .NET Runtime errors
3. Common causes:
   - Missing .NET 8.0 Hosting Bundle
   - Invalid connection string from Key Vault
   - Incorrect environment variable syntax in web.config

### Recycle App Pools

```powershell
# Use Windows PowerShell 5.1 (not PowerShell 7)
Import-Module WebAdministration
Restart-WebAppPool -Name "aidcircle-web"
Restart-WebAppPool -Name "aidcircle-api"

# Wait 10 seconds, then check logs
Start-Sleep -Seconds 10
Get-Content "C:\inetpub\wwwroot\aidcircle-web\logs\stdout_*.log" -Tail 30
```

### PowerShell Version Issues

If using PowerShell 7, IIS cmdlets may not work. Use Windows PowerShell 5.1:

```powershell
$psPath = "$env:SystemRoot\System32\WindowsPowerShell\v1.0\powershell.exe"
& $psPath -Command "Import-Module WebAdministration; Restart-WebAppPool -Name 'aidcircle-web'"
```

## Security Best Practices

1. **Never commit secrets to source control**
   - Add `appsettings.Development.Local.json` to `.gitignore`
   - Add `web.config` with real secrets to `.gitignore`
   - Use configuration templates (`.template` suffix) in repo

2. **Restrict web.config permissions**
   ```powershell
   # Set NTFS permissions on web.config
   icacls "C:\inetpub\wwwroot\aidcircle-web\web.config" /inheritance:r
   icacls "C:\inetpub\wwwroot\aidcircle-web\web.config" /grant:r "IIS AppPool\aidcircle-web:(R)"
   icacls "C:\inetpub\wwwroot\aidcircle-web\web.config" /grant:r "BUILTIN\Administrators:(F)"
   ```

3. **Rotate client secrets regularly**
   - Create new secret in Azure AD app registration
   - Update web.config
   - Recycle app pool
   - Delete old secret after verification

4. **Use managed identity when possible**
   - Azure App Service: System-assigned managed identity works natively
   - Azure Container Apps: Managed identity works well
   - **IIS on-premises**: App Registration with client secret recommended (Arc MI unreliable)

## Key Vault Secret Management

### Required Secrets

Add these secrets to your Key Vault:

```powershell
# Database connection string
az keyvault secret set `
  --vault-name <YOUR_KEYVAULT> `
  --name "H4HDB-DEV" `
  --value "Server=<server>.database.windows.net;Database=<db>;..."

# Azure OpenAI API Key
az keyvault secret set `
  --vault-name <YOUR_KEYVAULT> `
  --name "AzureOpenAI--ApiKey" `
  --value "<your-key>"

# Azure Translator Key
az keyvault secret set `
  --vault-name <YOUR_KEYVAULT> `
  --name "AzureTranslator--Key" `
  --value "<your-key>"
```

### Secret Naming Convention

Key Vault secret names use double dash (`--`) to represent nested configuration:

```
AzureOpenAI--ApiKey     → AzureOpenAI:ApiKey in appsettings.json
AzureTranslator--Key    → AzureTranslator:Key in appsettings.json
ConnectionStrings--H4HDB-DEV → ConnectionStrings:H4HDB-DEV
```

## Why Not Azure Arc Managed Identity?

**We attempted Azure Arc Managed Identity but encountered critical issues**:

1. **HIMDS Challenge-Response**: Arc's Hybrid Instance Metadata Service (HIMDS) uses a challenge-response authentication mechanism:
   - First request returns challenge token file path
   - Application must read token from disk (`C:\ProgramData\AzureConnectedMachineAgent\tokens\`)
   - Second request includes token in Authorization header
   
2. **IIS Worker Process Isolation**: IIS app pool worker processes run with restricted permissions and may not have access to the challenge token directory

3. **DefaultAzureCredential Complexity**: Even with proper configuration, the credential chain in IIS proved unreliable compared to straightforward client secret authentication

4. **Production Reliability**: Client ID + Secret provides predictable, well-tested authentication that works consistently across environments

**Recommendation**: Use App Registration with client secret for IIS deployments. Reserve managed identity for Azure-native compute (App Service, Container Apps, VMs).

## Summary

✅ **What Works**:
- App Registration with Client ID + Secret
- Environment variables in web.config
- Azure Key Vault RBAC role assignments
- DefaultAzureCredential with explicit options

❌ **What Doesn't Work Reliably**:
- Azure Arc Managed Identity on IIS (challenge token access issues)
- Relying on DefaultAzureCredential's automatic credential discovery without explicit options
- Storing secrets directly in appsettings.json (security risk)

**Final Pattern**:
```
App Registration → Client Secret in web.config → DefaultAzureCredential → Key Vault → Secrets → Application
```

This approach provides:
- ✅ Reliable authentication
- ✅ Secure secret management (secrets in Key Vault, not code)
- ✅ Simple troubleshooting (clear error messages)
- ✅ Works in all environments (dev, staging, production)
