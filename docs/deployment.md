# Deployment Guide

## Table of Contents
- [Prerequisites](#prerequisites)
- [Azure Resource Setup](#azure-resource-setup)
- [Key Vault Configuration](#key-vault-configuration)
- [Database Deployment](#database-deployment)
- [Application Deployment](#application-deployment)
- [Post-Deployment Verification](#post-deployment-verification)
- [CI/CD Pipeline](#cicd-pipeline)
- [Alternative: IIS with Azure Arc](#alternative-iis-with-azure-arc)

## Prerequisites

### Azure Account Requirements

✅ **Active Azure Subscription** with appropriate permissions:
- Contributor role (or higher) on subscription or resource group
- Ability to create App Services, SQL Databases, OpenAI resources

✅ **Azure CLI** installed and authenticated:
```powershell
# Install Azure CLI
winget install Microsoft.AzureCLI

# Login to Azure
az login

# Set subscription
az account set --subscription "YOUR_SUBSCRIPTION_ID"
```

✅ **Azure Resources Created**:
- Azure SQL Database
- Azure OpenAI with GPT-4o-mini deployment
- Azure AI Translator resource
- Azure AD External ID (B2C) tenant configured
- Azure Key Vault (for production secrets)

## Azure Resource Setup

### 1. Create Resource Group

```powershell
az group create `
  --name "rg-aidcircle-prod" `
  --location "eastus"
```

### 2. Create Azure SQL Database

```powershell
# Create SQL Server
az sql server create `
  --name "sql-aidcircle-prod" `
  --resource-group "rg-aidcircle-prod" `
  --location "eastus" `
  --admin-user "sqladmin" `
  --admin-password "YOUR_SECURE_PASSWORD"

# Create database
az sql db create `
  --resource-group "rg-aidcircle-prod" `
  --server "sql-aidcircle-prod" `
  --name "sqldb-aidcircle-prod" `
  --service-objective "S0" `
  --backup-storage-redundancy "Local"

# Configure firewall (allow Azure services)
az sql server firewall-rule create `
  --resource-group "rg-aidcircle-prod" `
  --server "sql-aidcircle-prod" `
  --name "AllowAzureServices" `
  --start-ip-address "0.0.0.0" `
  --end-ip-address "0.0.0.0"
```

### 3. Create App Service Plan & Web Apps

```powershell
# Create App Service Plan (Linux, .NET 8)
az appservice plan create `
  --name "asp-aidcircle-prod" `
  --resource-group "rg-aidcircle-prod" `
  --location "eastus" `
  --sku "B1" `
  --is-linux

# Create Web App (Blazor)
az webapp create `
  --name "app-aidcircle-web-prod" `
  --resource-group "rg-aidcircle-prod" `
  --plan "asp-aidcircle-prod" `
  --runtime "DOTNETCORE:8.0"

# Create API App
az webapp create `
  --name "app-aidcircle-api-prod" `
  --resource-group "rg-aidcircle-prod" `
  --plan "asp-aidcircle-prod" `
  --runtime "DOTNETCORE:8.0"
```

### 4. Enable Managed Identity

```powershell
# Enable system-assigned managed identity for Web App
az webapp identity assign `
  --name "app-aidcircle-web-prod" `
  --resource-group "rg-aidcircle-prod"

# Enable for API App
az webapp identity assign `
  --name "app-aidcircle-api-prod" `
  --resource-group "rg-aidcircle-prod"
```

**Note the Principal ID** from output - you'll need it for Key Vault access.

## Key Vault Configuration

### Create Key Vault

```powershell
az keyvault create `
  --name "kv-aidcircle-prod" `
  --resource-group "rg-aidcircle-prod" `
  --location "eastus" `
  --enable-rbac-authorization false
```

### Grant Access to Managed Identities

```powershell
# Get Web App's Principal ID
$webPrincipalId = az webapp identity show `
  --name "app-aidcircle-web-prod" `
  --resource-group "rg-aidcircle-prod" `
  --query principalId -o tsv

# Get API App's Principal ID
$apiPrincipalId = az webapp identity show `
  --name "app-aidcircle-api-prod" `
  --resource-group "rg-aidcircle-prod" `
  --query principalId -o tsv

# Grant Web App access to Key Vault
az keyvault set-policy `
  --name "kv-aidcircle-prod" `
  --object-id $webPrincipalId `
  --secret-permissions get list

# Grant API App access to Key Vault
az keyvault set-policy `
  --name "kv-aidcircle-prod" `
  --object-id $apiPrincipalId `
  --secret-permissions get list
```

### Store Secrets in Key Vault

```powershell
# SQL Connection String
az keyvault secret set `
  --vault-name "kv-aidcircle-prod" `
  --name "ConnectionStrings--H4HDB-DEV" `
  --value "Server=sql-aidcircle-prod.database.windows.net;Database=sqldb-aidcircle-prod;Authentication=Active Directory Managed Identity;TrustServerCertificate=True;"

# Azure OpenAI API Key
az keyvault secret set `
  --vault-name "kv-aidcircle-prod" `
  --name "AzureOpenAI--ApiKey" `
  --value "YOUR_AZURE_OPENAI_API_KEY"

# Azure Translator API Key
az keyvault secret set `
  --vault-name "kv-aidcircle-prod" `
  --name "AzureTranslator--Key" `
  --value "YOUR_AZURE_TRANSLATOR_API_KEY"

# Azure AD B2C Client Secret
az keyvault secret set `
  --vault-name "kv-aidcircle-prod" `
  --name "AzureAd--ClientSecret" `
  --value "YOUR_AZURE_AD_CLIENT_SECRET"
```

**Secret Naming Convention**: Azure Key Vault uses `--` as section delimiter (not `:` like appsettings.json).

### Update Application Configuration

Add Key Vault reference to both Web and API apps:

```powershell
# Configure Web App to use Key Vault
az webapp config appsettings set `
  --name "app-aidcircle-web-prod" `
  --resource-group "rg-aidcircle-prod" `
  --settings KeyVaultName="kv-aidcircle-prod"

# Configure API App to use Key Vault
az webapp config appsettings set `
  --name "app-aidcircle-api-prod" `
  --resource-group "rg-aidcircle-prod" `
  --settings KeyVaultName="kv-aidcircle-prod"
```

## Database Deployment

### Option 1: Run Migrations from Local Machine

```powershell
# Set connection string environment variable
$env:ConnectionStrings__H4HDB-DEV = "Server=sql-aidcircle-prod.database.windows.net;Database=sqldb-aidcircle-prod;User Id=sqladmin;Password=YOUR_PASSWORD;TrustServerCertificate=True;"

# Apply migrations
dotnet ef database update `
  --project H4H.Infrastructure `
  --startup-project H4H.Presentation.API `
  --connection $env:ConnectionStrings__H4HDB-DEV
```

### Option 2: Generate SQL Script for Manual Execution

```powershell
# Generate migration script
dotnet ef migrations script `
  --project H4H.Infrastructure `
  --startup-project H4H.Presentation.API `
  --output migration.sql

# Execute in Azure Data Studio or SSMS
sqlcmd -S sql-aidcircle-prod.database.windows.net `
  -d sqldb-aidcircle-prod `
  -U sqladmin `
  -P YOUR_PASSWORD `
  -i migration.sql
```

### Option 3: Automatic Migration on Startup (Not Recommended for Production)

Add to `Program.cs` (use with caution):

```csharp
if (app.Environment.IsProduction())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<H4HDbContext>();
    await dbContext.Database.MigrateAsync(); // ⚠️ Use only in controlled environments
}
```

## Application Deployment

### Build and Publish

```powershell
# Clean previous builds
dotnet clean H4H.sln --configuration Release

# Restore dependencies
dotnet restore H4H.sln

# Build in Release mode
dotnet build H4H.sln --configuration Release --no-restore

# Publish API
dotnet publish H4H.Presentation.API/H4H.Presentation.API.csproj `
  --configuration Release `
  --output ./publish/api

# Publish Web
dotnet publish H4H.Presentation.Web/H4H.Presentation.Web/H4H.Presentation.Web.csproj `
  --configuration Release `
  --output ./publish/web
```

### Deploy to Azure App Service

#### Option A: Azure CLI Deployment

```powershell
# Deploy API
az webapp deploy `
  --name "app-aidcircle-api-prod" `
  --resource-group "rg-aidcircle-prod" `
  --src-path "./publish/api" `
  --type zip

# Deploy Web
az webapp deploy `
  --name "app-aidcircle-web-prod" `
  --resource-group "rg-aidcircle-prod" `
  --src-path "./publish/web" `
  --type zip
```

#### Option B: ZIP Deployment via Kudu

```powershell
# Create ZIP files
Compress-Archive -Path ./publish/api/* -DestinationPath api.zip
Compress-Archive -Path ./publish/web/* -DestinationPath web.zip

# Deploy using Kudu API (requires publish credentials)
Invoke-RestMethod -Uri "https://app-aidcircle-api-prod.scm.azurewebsites.net/api/zipdeploy" `
  -Method POST `
  -InFile "api.zip" `
  -ContentType "application/zip" `
  -Headers @{Authorization = "Basic YOUR_PUBLISH_CREDENTIALS"}
```

#### Option C: Visual Studio Publish Profile

1. Right-click project → **Publish**
2. Choose **Azure App Service (Linux)**
3. Select your subscription and app
4. Click **Publish**

### Configure Application Settings

```powershell
# Set production environment variables
az webapp config appsettings set `
  --name "app-aidcircle-web-prod" `
  --resource-group "rg-aidcircle-prod" `
  --settings `
    ASPNETCORE_ENVIRONMENT="Production" `
    AzureOpenAI__Endpoint="https://YOUR-RESOURCE.openai.azure.com/" `
    AzureOpenAI__DeploymentName="gpt-4o-mini" `
    AzureTranslator__Endpoint="https://api.cognitive.microsofttranslator.com" `
    AzureTranslator__Region="eastus" `
    AzureAdB2C__Instance="https://YOUR-TENANT.b2clogin.com/" `
    AzureAdB2C__Domain="YOUR-TENANT.onmicrosoft.com" `
    AzureAdB2C__ClientId="YOUR_CLIENT_ID" `
    AzureAdB2C__SignUpSignInPolicyId="B2C_1_susi"
```

## Post-Deployment Verification

### Health Check Endpoints

Create health check endpoints for monitoring:

**Add to `Program.cs`**:

```csharp
app.MapHealthChecks("/health");
app.MapGet("/api/health/detailed", async (H4HDbContext db) =>
{
    try
    {
        await db.Database.CanConnectAsync();
        return Results.Ok(new { Status = "Healthy", Database = "Connected" });
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: 503);
    }
});
```

**Test health checks**:

```powershell
# Test Web App
Invoke-RestMethod -Uri "https://app-aidcircle-web-prod.azurewebsites.net/health"

# Test API
Invoke-RestMethod -Uri "https://app-aidcircle-api-prod.azurewebsites.net/health"
```

### Verify Key Vault Integration

Check application logs for successful Key Vault connection:

```powershell
az webapp log tail `
  --name "app-aidcircle-web-prod" `
  --resource-group "rg-aidcircle-prod"
```

Look for:
```
info: Program[0]
      ✅ Connected to Azure Key Vault: https://kv-aidcircle-prod.vault.azure.net/
```

### Test Database Connection

```powershell
# Run query via App Service console
az webapp ssh `
  --name "app-aidcircle-api-prod" `
  --resource-group "rg-aidcircle-prod"

# Inside SSH session
dotnet exec H4H.Presentation.API.dll --check-database
```

### Verify Authentication Flow

1. Navigate to `https://app-aidcircle-web-prod.azurewebsites.net`
2. Click **Sign In**
3. Verify redirect to Azure AD B2C
4. Complete sign-in flow
5. Verify redirect back to application

## CI/CD Pipeline

### GitHub Actions Workflow

Create `.github/workflows/deploy-production.yml`:

```yaml
name: Deploy to Production

on:
  push:
    branches: [main]
  workflow_dispatch:

env:
  DOTNET_VERSION: '8.0.x'
  AZURE_WEBAPP_NAME_API: 'app-aidcircle-api-prod'
  AZURE_WEBAPP_NAME_WEB: 'app-aidcircle-web-prod'

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: ${{ env.DOTNET_VERSION }}
    
    - name: Restore dependencies
      run: dotnet restore H4H.sln
    
    - name: Build solution
      run: dotnet build H4H.sln --configuration Release --no-restore
    
    - name: Run tests
      run: dotnet test H4H.sln --configuration Release --no-build --verbosity normal
    
    - name: Publish API
      run: dotnet publish H4H.Presentation.API/H4H.Presentation.API.csproj --configuration Release --output ./publish/api
    
    - name: Publish Web
      run: dotnet publish H4H.Presentation.Web/H4H.Presentation.Web/H4H.Presentation.Web.csproj --configuration Release --output ./publish/web
    
    - name: Deploy API to Azure
      uses: azure/webapps-deploy@v2
      with:
        app-name: ${{ env.AZURE_WEBAPP_NAME_API }}
        publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE_API }}
        package: ./publish/api
    
    - name: Deploy Web to Azure
      uses: azure/webapps-deploy@v2
      with:
        app-name: ${{ env.AZURE_WEBAPP_NAME_WEB }}
        publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE_WEB }}
        package: ./publish/web
```

### Get Publish Profiles

```powershell
# Download API publish profile
az webapp deployment list-publishing-profiles `
  --name "app-aidcircle-api-prod" `
  --resource-group "rg-aidcircle-prod" `
  --xml > api-publish-profile.xml

# Download Web publish profile
az webapp deployment list-publishing-profiles `
  --name "app-aidcircle-web-prod" `
  --resource-group "rg-aidcircle-prod" `
  --xml > web-publish-profile.xml
```

Add as GitHub Secrets:
- `AZURE_WEBAPP_PUBLISH_PROFILE_API`
- `AZURE_WEBAPP_PUBLISH_PROFILE_WEB`

### Database Migration in CI/CD

Add migration step before deployment:

```yaml
- name: Apply Database Migrations
  run: |
    dotnet tool install --global dotnet-ef
    dotnet ef database update --project H4H.Infrastructure --startup-project H4H.Presentation.API
  env:
    ConnectionStrings__H4HDB-DEV: ${{ secrets.PRODUCTION_CONNECTION_STRING }}
```

## Monitoring & Logging

### Enable Application Insights

```powershell
# Create Application Insights
az monitor app-insights component create `
  --app "appinsights-aidcircle-prod" `
  --location "eastus" `
  --resource-group "rg-aidcircle-prod" `
  --application-type web

# Get instrumentation key
$instrumentationKey = az monitor app-insights component show `
  --app "appinsights-aidcircle-prod" `
  --resource-group "rg-aidcircle-prod" `
  --query instrumentationKey -o tsv

# Configure Web App
az webapp config appsettings set `
  --name "app-aidcircle-web-prod" `
  --resource-group "rg-aidcircle-prod" `
  --settings APPINSIGHTS_INSTRUMENTATIONKEY=$instrumentationKey
```

### View Logs

```powershell
# Stream logs in real-time
az webapp log tail `
  --name "app-aidcircle-web-prod" `
  --resource-group "rg-aidcircle-prod"

# Download logs
az webapp log download `
  --name "app-aidcircle-web-prod" `
  --resource-group "rg-aidcircle-prod" `
  --log-file app-logs.zip
```

## Rollback Procedure

### Swap Deployment Slots

```powershell
# Create staging slot
az webapp deployment slot create `
  --name "app-aidcircle-web-prod" `
  --resource-group "rg-aidcircle-prod" `
  --slot "staging"

# Deploy to staging first
az webapp deploy `
  --name "app-aidcircle-web-prod" `
  --resource-group "rg-aidcircle-prod" `
  --slot "staging" `
  --src-path "./publish/web" `
  --type zip

# Test staging: https://app-aidcircle-web-prod-staging.azurewebsites.net

# Swap to production
az webapp deployment slot swap `
  --name "app-aidcircle-web-prod" `
  --resource-group "rg-aidcircle-prod" `
  --slot "staging" `
  --target-slot "production"
```

### Database Rollback

```powershell
# Rollback to specific migration
dotnet ef database update PreviousMigrationName `
  --project H4H.Infrastructure `
  --startup-project H4H.Presentation.API
```

## Security Checklist

- ✅ All secrets stored in Azure Key Vault (never in appsettings.json)
- ✅ Managed Identity enabled for Key Vault access
- ✅ SQL Server firewall configured (only allow Azure services)
- ✅ HTTPS enforced on App Services
- ✅ Azure AD B2C authentication configured
- ✅ CORS properly configured (production URLs only)
- ✅ Application Insights monitoring enabled
- ✅ Diagnostic logging enabled

## Cost Optimization

**Production Resources (Monthly Estimate)**:

| Resource | SKU | Cost |
|----------|-----|------|
| App Service Plan | B1 (Basic) | ~$13 |
| Azure SQL Database | S0 (Standard) | ~$15 |
| Azure OpenAI | Pay-per-use (1M tokens) | ~$10 |
| Azure Translator | Pay-per-use (1M chars) | ~$10 |
| Azure Key Vault | Standard | ~$0.03 |
| **Total** | | **~$48/month** |

**Cost Reduction Tips**:
- Use Azure Dev/Test pricing for non-production
- Scale down during off-hours
- Use reserved capacity for predictable workloads

---

## Alternative: IIS with Azure Arc

If you prefer to host AidCircle on **your own Windows Server** while still leveraging Azure services, see the **[IIS + Azure Arc Deployment Guide](deployment-iis-arc.md)**.

**When to use IIS + Azure Arc**:
- ✅ You own Windows Server infrastructure
- ✅ You want control over the hosting environment
- ✅ You need to comply with on-premises requirements
- ✅ You still want Azure Key Vault integration via managed identity

**Key differences from App Service**:
- You manage server updates and security patches
- Lower monthly cost (~$5-10/month Azure Arc + server costs)
- Full control over IIS configuration
- Same managed identity capabilities as Azure App Service

See the complete guide at: **[deployment-iis-arc.md](deployment-iis-arc.md)**

---

## Next Steps

- 📚 **Understand the Architecture** → Read [Architecture Overview](architecture-overview.md)
- 💬 **Explore AI Chat** → See [AI Chat Deep Dive](ai-chat.md)
- 💻 **Local Development** → Follow [Local Development Guide](local-development.md)
- 🖥️ **Deploy to IIS** → Try [IIS + Azure Arc Deployment](deployment-iis-arc.md)
