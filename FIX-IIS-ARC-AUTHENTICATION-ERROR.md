# 🚨 URGENT: Fix DefaultAzureCredential Error on IIS with Azure Arc

## Your Current Error

```
Application '/LM/W3SVC/33/ROOT' with physical root 'E:\aidcircle\web\' hit unexpected managed exception, exception code = '0xe0434352'. 
Unhandled exception. Azure.Identity.CredentialUnavailableException: DefaultAzureCredential failed to retrieve a token from the included credentials.
- EnvironmentCredential authentication unavailable. Environment variables are not fully configured.
- WorkloadIdentityCredential authentication unavailable. The workload options are not fully configured.
```

## Root Cause

Your IIS application can't authenticate to Azure services (Key Vault, Azure OpenAI, etc.) because:

1. ❌ **Missing `AZURE_CLIENT_ID` environment variable** - Arc managed identity not detected
2. ❌ **Environment variables set at MACHINE level** - Not application-specific (security risk)
3. ⚠️ **Arc HIMDS service** may not be running or detected

---

## ✅ Solution: 3-Step Fix

### Step 1: Get Your Arc Managed Identity Principal ID

**On your IIS server**, run this PowerShell command:

```powershell
# Replace with your actual values
$machineName = "YOUR-IIS-SERVER-HOSTNAME"  # e.g., the computer name
$resourceGroup = "rg-aidcircle-arc"        # Your Azure resource group where Arc server is registered

# Get the Principal ID
az connectedmachine show `
  --name $machineName `
  --resource-group $resourceGroup `
  --query "identity.principalId" -o tsv
```

**Expected Output** (copy this value):
```
a1b2c3d4-e5f6-7890-abcd-ef1234567890
```

**Alternative if Azure CLI not installed**:
- Go to **Azure Portal** → Search "Azure Arc" → "Servers"
- Find your server → Click **Identity** blade
- Copy the **Object (principal) ID**

---

### Step 2: Update web.config with Application-Specific Variables

Edit `E:\aidcircle\web\web.config` (or wherever your app is deployed):

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
        
        <!-- ⭐ ADD THIS SECTION ⭐ -->
        <environmentVariables>
          <!-- CRITICAL: Arc Managed Identity Client ID -->
          <environmentVariable name="AZURE_CLIENT_ID" value="PASTE_YOUR_PRINCIPAL_ID_HERE" />
          
          <!-- Required Configuration -->
          <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
          <environmentVariable name="KeyVaultName" value="kv-aidcircle-prod" />
          
          <!-- Azure AD B2C -->
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
          
          <!-- API Client -->
          <environmentVariable name="ApiSettings__BaseUrl" value="http://localhost:5135" />
        </environmentVariables>
        
      </aspNetCore>
    </system.webServer>
  </location>
</configuration>
```

**IMPORTANT**: Replace `PASTE_YOUR_PRINCIPAL_ID_HERE` with the actual value from Step 1.

Do the same for `E:\aidcircle\api\web.config` (API app).

---

### Step 3: Recycle Application Pool

```powershell
# Restart ONLY the affected app pool (no full IIS restart needed)
Restart-WebAppPool -Name "AidCircleWebPool"

# Verify it restarted
Get-WebAppPoolState -Name "AidCircleWebPool"
# Should show: Started
```

---

## ✅ Verify the Fix

### Test 1: Check HIMDS Service (Arc Agent)

```powershell
# Ensure Arc agent is running
Get-Service himds

# If not running, start it
Start-Service himds

# Verify connection to Azure
azcmagent show
# Should show: Agent Status: Connected
```

### Test 2: Test Managed Identity Endpoint

```powershell
# Test if Arc HIMDS is responding
$tokenUrl = "http://localhost:40342/metadata/identity/oauth2/token?api-version=2020-06-01&resource=https://vault.azure.net"
$response = Invoke-RestMethod -Uri $tokenUrl -Headers @{Metadata="true"} -UseBasicParsing

# Should return JSON with access_token field
$response.access_token
```

If this fails, Arc managed identity isn't working. Check:
- Arc agent is installed: `azcmagent --version`
- Arc agent is connected: `azcmagent show`
- HIMDS service is running: `Get-Service himds`

### Test 3: Check Application Logs

```powershell
# View latest stdout logs
Get-Content "E:\aidcircle\web\logs\stdout_*.log" -Tail 100

# Look for:
# ✅ "Hosting environment: Production"
# ✅ No "Azure.Identity.CredentialUnavailableException" errors
```

### Test 4: Browse Application

Open browser to: `http://localhost:5011` (or your configured port)

If it loads without errors, the fix worked! 🎉

---

## 🔐 Grant Key Vault Access to Arc Identity

Once the above steps work, ensure the Arc managed identity can access Key Vault:

```powershell
$vaultName = "kv-aidcircle-prod"
$principalId = "YOUR_PRINCIPAL_ID_FROM_STEP_1"

# Grant access
az keyvault set-policy `
  --name $vaultName `
  --object-id $principalId `
  --secret-permissions get list

# Verify
az keyvault show `
  --name $vaultName `
  --query "properties.accessPolicies[?objectId=='$principalId']"
```

---

## 🚫 Remove Machine-Level Environment Variables (Security Fix)

If you previously set machine-level variables, **REMOVE THEM** (they're a security risk):

```powershell
# List all machine-level variables (to see what needs removal)
[Environment]::GetEnvironmentVariables("Machine") | Format-Table

# Remove specific variables (example)
[Environment]::SetEnvironmentVariable("AZURE_CLIENT_ID", $null, "Machine")
[Environment]::SetEnvironmentVariable("KeyVaultName", $null, "Machine")
[Environment]::SetEnvironmentVariable("AzureOpenAI__Endpoint", $null, "Machine")
# ... remove any others you set ...

# Restart IIS to clear cached variables
iisreset
```

**Why remove them?**
- ❌ All apps on the server can read them (security vulnerability)
- ❌ Other users can access secrets via `$env:VariableName`
- ✅ Use `web.config` for application-specific, isolated configuration

---

## 🛠️ Automated Setup Script

For easier setup, use the provided PowerShell script:

```powershell
# Run this on your IIS server (as Administrator)
cd E:\aidcircle

# Download the script (if not already in repo)
# Or copy Configure-IISArcIdentity.ps1 from the repo

# Run the script
.\Configure-IISArcIdentity.ps1 `
  -MachineName "YOUR-SERVER-HOSTNAME" `
  -ResourceGroup "rg-aidcircle-arc" `
  -WebAppPath "E:\aidcircle\web" `
  -ApiAppPath "E:\aidcircle\api" `
  -KeyVaultName "kv-aidcircle-prod" `
  -AzureOpenAIEndpoint "https://YOUR-RESOURCE.openai.azure.com/"

# The script will:
# ✅ Retrieve managed identity Principal ID
# ✅ Update both web.config files
# ✅ Grant Key Vault access
# ✅ Recycle app pools
```

---

## 📋 Quick Checklist

Before your app will work:

- [ ] Azure Arc agent installed (`azcmagent --version`)
- [ ] Arc agent connected to Azure (`azcmagent show` → Status: Connected)
- [ ] HIMDS service running (`Get-Service himds`)
- [ ] Managed identity enabled on Arc server (in Azure Portal)
- [ ] `AZURE_CLIENT_ID` in `web.config` (NOT machine-level)
- [ ] `KeyVaultName` in `web.config`
- [ ] All other required config in `web.config`
- [ ] App pool recycled after `web.config` changes
- [ ] Key Vault access policy granted to managed identity
- [ ] Logs directory exists (`E:\aidcircle\web\logs`)

---

## 🆘 Still Getting Errors?

### Error: "HIMDS service not found"

**Solution**: Install Azure Arc agent:

```powershell
# Download and run Arc onboarding script from Azure Portal
# Portal → Azure Arc → Servers → Add → Generate script
```

### Error: "403 Forbidden" from Key Vault

**Solution**: Grant access policy (see "Grant Key Vault Access" section above)

### Error: "Connection refused localhost:40342"

**Solution**: HIMDS service not running:

```powershell
Start-Service himds
# Or reinstall Arc agent
```

### Error: Variables still not loading

**Solution**: XML syntax error in `web.config`:

```powershell
# Validate XML
[xml](Get-Content "E:\aidcircle\web\web.config")

# If error, check:
# - All tags properly closed
# - <environmentVariables> inside <aspNetCore>
# - No special characters in values (escape & as &amp;)
```

---

## 📚 Additional Resources

- **Full Guide**: [IIS-ENVIRONMENT-VARIABLES-GUIDE.md](IIS-ENVIRONMENT-VARIABLES-GUIDE.md)
- **IIS Deployment**: [docs/deployment-iis-arc.md](docs/deployment-iis-arc.md)
- **Secrets Management**: [SECRETS-MANAGEMENT-IMPLEMENTATION.md](SECRETS-MANAGEMENT-IMPLEMENTATION.md)
- **Azure Arc Docs**: https://learn.microsoft.com/azure/azure-arc/servers/managed-identity-authentication

---

## Summary

**Your issue**: `DefaultAzureCredential` can't find credentials because `AZURE_CLIENT_ID` is missing.

**The fix**:
1. Get Arc managed identity Principal ID from Azure
2. Add it to `web.config` as `AZURE_CLIENT_ID` environment variable
3. Recycle app pool

**Why this works**: `DefaultAzureCredential` tries multiple auth methods in this order:
1. EnvironmentCredential (requires `AZURE_CLIENT_ID`)
2. ManagedIdentityCredential (Arc HIMDS)

By setting `AZURE_CLIENT_ID`, you're telling it to use Arc's managed identity.

**Security bonus**: Using `web.config` keeps secrets isolated per-application (not machine-wide).

Good luck! 🚀
