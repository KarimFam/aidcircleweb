# Local Development Setup Guide

This guide explains how to set up AidCircle for local development with proper secrets management.

## Overview

AidCircle uses a two-tier secrets management approach:

1. **Local Development**: Git-ignored `appsettings.Development.Local.json` files for developer-specific secrets
2. **Production**: Azure Key Vault with `DefaultAzureCredential` for secure secrets access

## Quick Start for New Developers

### Step 1: Clone the Repository

```powershell
git clone https://github.com/KarimFam/aidcircleweb.git
cd aidcircleweb
```

### Step 2: Create Local Secrets Files

You need to create local secrets files for both the API and Web projects. These files are **git-ignored** and will never be committed to source control.

#### For API Project

1. Copy the template:
   ```powershell
   Copy-Item "H4H.Presentation.API\appsettings.Development.Local.json.template" "H4H.Presentation.API\appsettings.Development.Local.json"
   ```

2. Edit `H4H.Presentation.API\appsettings.Development.Local.json` and replace placeholders with your actual values:

```json
{
  "ConnectionStrings": {
    "H4HDB-DEV": "Server=YOUR_SQL_SERVER;Database=YOUR_DATABASE;User Id=YOUR_USERNAME;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
  },
  "AzureOpenAI": {
    "ApiKey": "YOUR_AZURE_OPENAI_API_KEY"
  },
  "AzureTranslator": {
    "Key": "YOUR_AZURE_TRANSLATOR_API_KEY"
  }
}
```

#### For Web Project

1. Copy the template:
   ```powershell
   Copy-Item "H4H.Presentation.Web\H4H.Presentation.Web\appsettings.Development.Local.json.template" "H4H.Presentation.Web\H4H.Presentation.Web\appsettings.Development.Local.json"
   ```

2. Edit `H4H.Presentation.Web\H4H.Presentation.Web\appsettings.Development.Local.json` and replace placeholders:

```json
{
  "ConnectionStrings": {
    "H4HDB-DEV": "Server=YOUR_SQL_SERVER;Database=YOUR_DATABASE;User Id=YOUR_USERNAME;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
  },
  "AzureAd": {
    "ClientSecret": "YOUR_AZURE_AD_CLIENT_SECRET"
  },
  "AzureOpenAI": {
    "ApiKey": "YOUR_AZURE_OPENAI_API_KEY"
  },
  "AzureTranslator": {
    "Key": "YOUR_AZURE_TRANSLATOR_API_KEY"
  }
}
```

### Step 3: Obtain Secrets

Contact the project administrator to obtain:

- **SQL Server Connection String**: Database server, credentials, and database name
- **Azure OpenAI API Key**: From Azure Portal → Azure OpenAI resource → Keys and Endpoint
- **Azure Translator API Key**: From Azure Portal → Translator resource → Keys and Endpoint
- **Azure AD Client Secret**: From Azure Portal → App Registration → Certificates & secrets

### Step 4: Run Database Migrations

```powershell
dotnet ef database update --project H4H.Infrastructure --startup-project H4H.Presentation.API
```

### Step 5: Run the Application

```powershell
# Terminal 1 - API
dotnet run --project H4H.Presentation.API

# Terminal 2 - Web
dotnet run --project H4H.Presentation.Web/H4H.Presentation.Web
```

## Configuration Priority

The configuration system loads settings in this order (later sources override earlier ones):

1. `appsettings.json` - Base configuration (committed to git)
2. `appsettings.Development.json` - Development overrides (committed to git)
3. **`appsettings.Development.Local.json`** - Local secrets (git-ignored) ⭐
4. Environment variables
5. Azure Key Vault (if `KeyVaultName` is configured)
6. User secrets (optional, see below)

## Alternative: Using .NET User Secrets

Instead of `appsettings.Development.Local.json`, you can use the built-in .NET User Secrets feature:

### Initialize User Secrets

```powershell
# For API project
dotnet user-secrets init --project H4H.Presentation.API

# For Web project
dotnet user-secrets init --project H4H.Presentation.Web/H4H.Presentation.Web
```

### Set Secrets

```powershell
# API Connection String
dotnet user-secrets set "ConnectionStrings:H4HDB-DEV" "Server=...;Database=...;" --project H4H.Presentation.API

# Azure OpenAI
dotnet user-secrets set "AzureOpenAI:ApiKey" "YOUR_KEY" --project H4H.Presentation.API

# Azure Translator
dotnet user-secrets set "AzureTranslator:Key" "YOUR_KEY" --project H4H.Presentation.API

# Repeat for Web project...
```

### View Secrets

```powershell
dotnet user-secrets list --project H4H.Presentation.API
```

**Note**: User secrets are stored outside the project directory at:
- Windows: `%APPDATA%\Microsoft\UserSecrets\<user_secrets_id>\secrets.json`
- macOS/Linux: `~/.microsoft/usersecrets/<user_secrets_id>/secrets.json`

## Production Deployment with Azure Key Vault

### Prerequisites

1. Azure Key Vault resource created (e.g., `aidcirclekeyvault`)
2. App Service or Azure resource with Managed Identity enabled
3. Managed Identity granted "Key Vault Secrets User" role

### Key Vault Secret Names

Store secrets in Key Vault with these exact names (colons replaced with double-dashes):

```
ConnectionStrings--H4HDB-DEV
AzureOpenAI--ApiKey
AzureTranslator--Key
AzureAd--ClientSecret
```

### How It Works

1. `appsettings.json` contains `"KeyVaultName": "aidcirclekeyvault"`
2. On startup, `Program.cs` detects the Key Vault name
3. `DefaultAzureCredential` authenticates:
   - **Local dev**: Uses Azure CLI login (`az login`)
   - **Production**: Uses Managed Identity automatically
4. All secrets from Key Vault override local configuration

### Testing Key Vault Locally

1. Install Azure CLI: https://aka.ms/install-azure-cli
2. Login to Azure:
   ```powershell
   az login
   ```
3. Ensure your account has "Key Vault Secrets User" role on the Key Vault
4. Remove or rename `appsettings.Development.Local.json` to force Key Vault usage
5. Run the application - it will fetch secrets from Azure

## Security Best Practices

### ✅ DO

- Use `appsettings.Development.Local.json` for local secrets
- Commit `appsettings.Development.Local.json.template` files (with placeholders)
- Use Azure Key Vault for production secrets
- Keep `DefaultAzureCredential` for authentication
- Rotate secrets regularly
- Grant least-privilege access to Key Vault

### ❌ DON'T

- Commit `appsettings.Development.Local.json` to git
- Hardcode secrets in source code
- Share secrets via email or chat
- Store secrets in comments or documentation
- Use production secrets in local development

## Troubleshooting

### "Connection string not found"

**Solution**: Ensure `appsettings.Development.Local.json` exists and contains `ConnectionStrings:H4HDB-DEV`

### "Azure Key Vault authentication failed"

**Solutions**:
1. Run `az login` to authenticate Azure CLI
2. Verify `KeyVaultName` in `appsettings.json` matches your Key Vault
3. Check your Azure account has "Key Vault Secrets User" role

### "Secret not found in Key Vault"

**Solutions**:
1. Verify secret name uses double-dashes (`--`) instead of colons (`:`)
2. Check secret exists: `az keyvault secret list --vault-name aidcirclekeyvault`
3. Ensure secret is not expired or disabled

### "DefaultAzureCredential failed"

**Error**: Multiple authentication methods failed

**Solutions**:
1. **Local dev**: Run `az login` and ensure you're logged into the correct tenant
2. **Production**: Verify Managed Identity is enabled on the App Service
3. Check network access: Ensure no firewall blocks Key Vault access

## Configuration Hierarchy Example

Given these files:

**appsettings.json**:
```json
{
  "KeyVaultName": "aidcirclekeyvault",
  "AzureOpenAI": {
    "Endpoint": "https://kfameastus2oai.openai.azure.com/",
    "DeploymentName": "kfameastus2oai"
  }
}
```

**appsettings.Development.Local.json**:
```json
{
  "ConnectionStrings": {
    "H4HDB-DEV": "Server=localhost;..."
  },
  "AzureOpenAI": {
    "ApiKey": "local-dev-key"
  }
}
```

**Azure Key Vault** (if authenticated):
```
AzureOpenAI--ApiKey = "production-key-from-vault"
```

**Final Configuration**:
```json
{
  "KeyVaultName": "aidcirclekeyvault",
  "ConnectionStrings": {
    "H4HDB-DEV": "Server=localhost;..."
  },
  "AzureOpenAI": {
    "Endpoint": "https://kfameastus2oai.openai.azure.com/",
    "DeploymentName": "kfameastus2oai",
    "ApiKey": "production-key-from-vault"  // ← Key Vault wins!
  }
}
```

## Team Onboarding Checklist

New developers should:

- [ ] Clone the repository
- [ ] Copy `.template` files to create `appsettings.Development.Local.json`
- [ ] Request secrets from team lead
- [ ] Populate local secrets files with actual values
- [ ] Run database migrations
- [ ] Verify API starts without errors
- [ ] Verify Web app starts and authenticates
- [ ] Test AI chat functionality
- [ ] **Never commit `appsettings.Development.Local.json` files**

## Additional Resources

- [Azure Key Vault Documentation](https://learn.microsoft.com/en-us/azure/key-vault/)
- [DefaultAzureCredential](https://learn.microsoft.com/en-us/dotnet/api/azure.identity.defaultazurecredential)
- [.NET User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)
- [Safe Storage of App Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)

## Contact

For access to secrets or Key Vault permissions, contact:
- Project Lead: [Your Name]
- DevOps Team: [Team Contact]
