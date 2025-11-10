# API Authentication & Secrets Management Guide

## Overview

AidCircle uses a **dual authentication strategy** for API communication:
1. **Azure AD B2C Bearer Tokens** - Primary authentication for user-initiated operations
2. **API Key** - Fallback authentication for service-to-service calls and background operations

All secrets are managed through **Azure Key Vault** with no hardcoded credentials.

---

## Architecture

```mermaid
flowchart TD
    User[User Browser] -->|HTTPS + B2C Token| Web[Blazor Web App]
    Web -->|API Call with B2C Token| API[REST API Backend]
    Web -->|API Call with API Key| API
    
    API -->|Validate B2C Token| AzureAD[Azure AD B2C]
    API -->|Validate API Key| Config[Configuration/Key Vault]
    
    Web -->|Load Secrets| KV1[Key Vault]
    API -->|Load Secrets| KV2[Key Vault]
    
    API -->|Managed Identity| SQL[(Azure SQL)]
    API -->|API Key| OpenAI[Azure OpenAI]
    API -->|API Key| Translator[Azure Translator]
    
    style Web fill:#9f6,stroke:#333
    style API fill:#69f,stroke:#333
    style KV1 fill:#f96,stroke:#333
    style KV2 fill:#f96,stroke:#333
```

---

## Required Secrets in Azure Key Vault

### Connection Strings

| Secret Name | Purpose | Example Value |
|-------------|---------|---------------|
| `ConnectionStrings--H4HDB-DEV` | Azure SQL Database connection | `Server=sql-aidcircle.database.windows.net;Database=aidcircle-db;Authentication=Active Directory Managed Identity;` |

### Azure OpenAI

| Secret Name | Purpose | Example Value |
|-------------|---------|---------------|
| `AzureOpenAI--ApiKey` | Azure OpenAI authentication | `your-azure-openai-api-key` |
| `AzureOpenAI--Endpoint` | Azure OpenAI endpoint URL | `https://your-resource.openai.azure.com/` |
| `AzureOpenAI--DeploymentName` | Model deployment name | `gpt-4o-mini` |

### Azure Translator

| Secret Name | Purpose | Example Value |
|-------------|---------|---------------|
| `AzureTranslator--Key` | Azure Translator API key | `your-translator-key` |
| `AzureTranslator--Endpoint` | Translator endpoint | `https://api.cognitive.microsofttranslator.com` |
| `AzureTranslator--Region` | Azure region | `eastus` |

### API Authentication

| Secret Name | Purpose | Example Value |
|-------------|---------|---------------|
| `ApiSettings--ApiKey` | Service-to-service API authentication | `generated-secure-api-key-32-chars-min` |

### Azure AD B2C (Web App Only)

| Secret Name | Purpose | Example Value |
|-------------|---------|---------------|
| `AzureAd--ClientSecret` | Azure AD B2C client secret | `your-b2c-client-secret` |

---

## Setting Up Secrets in Azure Key Vault

### Step 1: Create Key Vault (if not exists)

```powershell
# Create resource group
az group create --name rg-aidcircle-prod --location eastus

# Create Key Vault
az keyvault create \
  --name aidcirclekeyvault \
  --resource-group rg-aidcircle-prod \
  --location eastus \
  --enable-rbac-authorization false
```

### Step 2: Grant Access to Managed Identities

#### For Azure App Service (Web)

```powershell
# Enable managed identity
az webapp identity assign \
  --name app-aidcircle-web-prod \
  --resource-group rg-aidcircle-prod

# Get principal ID
$webPrincipalId = az webapp identity show \
  --name app-aidcircle-web-prod \
  --resource-group rg-aidcircle-prod \
  --query principalId -o tsv

# Grant Key Vault access
az keyvault set-policy \
  --name aidcirclekeyvault \
  --object-id $webPrincipalId \
  --secret-permissions get list
```

#### For Azure App Service (API)

```powershell
# Enable managed identity
az webapp identity assign \
  --name app-aidcircle-api-prod \
  --resource-group rg-aidcircle-prod

# Get principal ID
$apiPrincipalId = az webapp identity show \
  --name app-aidcircle-api-prod \
  --resource-group rg-aidcircle-prod \
  --query principalId -o tsv

# Grant Key Vault access
az keyvault set-policy \
  --name aidcirclekeyvault \
  --object-id $apiPrincipalId \
  --secret-permissions get list
```

#### For Azure Arc-Enabled IIS Server

```powershell
# Enable system-assigned managed identity on Arc server
az connectedmachine update \
  --name your-iis-server \
  --resource-group rg-aidcircle-prod \
  --set identity.type=SystemAssigned

# Get principal ID
$arcPrincipalId = az connectedmachine show \
  --name your-iis-server \
  --resource-group rg-aidcircle-prod \
  --query identity.principalId -o tsv

# Grant Key Vault access
az keyvault set-policy \
  --name aidcirclekeyvault \
  --object-id $arcPrincipalId \
  --secret-permissions get list
```

### Step 3: Add Secrets to Key Vault

```powershell
# Database connection string
az keyvault secret set \
  --vault-name aidcirclekeyvault \
  --name "ConnectionStrings--H4HDB-DEV" \
  --value "Server=sql-aidcircle.database.windows.net;Database=aidcircle-db;Authentication=Active Directory Managed Identity;TrustServerCertificate=True;"

# Azure OpenAI secrets
az keyvault secret set \
  --vault-name aidcirclekeyvault \
  --name "AzureOpenAI--ApiKey" \
  --value "YOUR_AZURE_OPENAI_API_KEY"

az keyvault secret set \
  --vault-name aidcirclekeyvault \
  --name "AzureOpenAI--Endpoint" \
  --value "https://your-resource.openai.azure.com/"

az keyvault secret set \
  --vault-name aidcirclekeyvault \
  --name "AzureOpenAI--DeploymentName" \
  --value "gpt-4o-mini"

# Azure Translator secrets
az keyvault secret set \
  --vault-name aidcirclekeyvault \
  --name "AzureTranslator--Key" \
  --value "YOUR_TRANSLATOR_KEY"

az keyvault secret set \
  --vault-name aidcirclekeyvault \
  --name "AzureTranslator--Endpoint" \
  --value "https://api.cognitive.microsofttranslator.com"

az keyvault secret set \
  --vault-name aidcirclekeyvault \
  --name "AzureTranslator--Region" \
  --value "eastus"

# Generate and store API key (use secure random generator)
$apiKey = -join ((48..57) + (65..90) + (97..122) | Get-Random -Count 32 | ForEach-Object {[char]$_})
az keyvault secret set \
  --vault-name aidcirclekeyvault \
  --name "ApiSettings--ApiKey" \
  --value $apiKey

Write-Host "Generated API Key: $apiKey" -ForegroundColor Green
Write-Host "⚠️  Store this key securely - you'll need it for local development!" -ForegroundColor Yellow

# Azure AD B2C client secret
az keyvault secret set \
  --vault-name aidcirclekeyvault \
  --name "AzureAd--ClientSecret" \
  --value "YOUR_AZURE_AD_B2C_CLIENT_SECRET"
```

---

## Local Development Setup

### Option 1: Azure CLI Authentication (Recommended)

**Prerequisites**:
- Azure CLI installed
- Logged in with `az login`
- Access to Key Vault granted to your Azure AD user

```powershell
# Login to Azure
az login

# Set subscription
az account set --subscription "YOUR_SUBSCRIPTION_ID"

# Grant yourself Key Vault access (if not already granted)
az keyvault set-policy \
  --name aidcirclekeyvault \
  --upn your-email@domain.com \
  --secret-permissions get list
```

**Application Configuration** (`appsettings.Development.json`):

```json
{
  "KeyVaultName": "aidcirclekeyvault",
  "ApiSettings": {
    "BaseUrl": "https://localhost:5135"
  }
}
```

The application will automatically use `DefaultAzureCredential` which tries:
1. Environment variables
2. Managed Identity
3. **Azure CLI credentials** (for local dev)
4. Visual Studio credentials
5. VS Code credentials

### Option 2: Local Secrets File (Fallback)

Create `appsettings.Development.Local.json` (git-ignored):

```json
{
  "ConnectionStrings": {
    "H4HDB-DEV": "Server=sql-aidcircle.database.windows.net;Database=aidcircle-db;User Id=sqladmin;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
  },
  "AzureOpenAI": {
    "ApiKey": "YOUR_AZURE_OPENAI_API_KEY",
    "Endpoint": "https://your-resource.openai.azure.com/",
    "DeploymentName": "gpt-4o-mini"
  },
  "AzureTranslator": {
    "Key": "YOUR_TRANSLATOR_KEY",
    "Endpoint": "https://api.cognitive.microsofttranslator.com",
    "Region": "eastus"
  },
  "ApiSettings": {
    "ApiKey": "YOUR_API_KEY_FROM_KEYVAULT",
    "BaseUrl": "https://localhost:5135"
  },
  "AzureAd": {
    "ClientSecret": "YOUR_AZURE_AD_CLIENT_SECRET"
  }
}
```

**⚠️ IMPORTANT**: Never commit this file to version control!

---

## Dual Authentication Flow

### User-Initiated Operations (Azure AD B2C Token)

```mermaid
sequenceDiagram
    participant User
    participant Web as Blazor Web
    participant API as REST API
    participant B2C as Azure AD B2C
    
    User->>Web: Click "Create Order"
    Web->>B2C: Validate user session
    B2C-->>Web: Return access token
    Web->>API: POST /api/orders (Bearer token in header)
    API->>B2C: Validate token
    B2C-->>API: Token valid ✓
    API-->>Web: Order created
    Web-->>User: Show confirmation
```

**Code Example (Web App)**:

```csharp
// ApiClient automatically adds B2C token
var order = await ApiClient.CreateOrderAsync(orderDto);
```

**API receives**:
```http
POST /api/orders HTTP/1.1
Host: api.aidcircle.net
Authorization: Bearer eyJ0eXAiOiJKV1QiLCJhbGc...
Content-Type: application/json

{
  "userId": "...",
  "items": [...]
}
```

### Service Operations (API Key)

```mermaid
sequenceDiagram
    participant Scheduler as Background Task
    participant Web as Blazor Web
    participant API as REST API
    participant KV as Key Vault
    
    Scheduler->>Web: Trigger "Get online volunteers"
    Web->>KV: Load API key
    KV-->>Web: Return API key
    Web->>API: GET /api/volunteers/online (X-API-Key header)
    API->>API: Validate API key
    API-->>Web: Return volunteers
    Web-->>Scheduler: Process data
```

**Code Example (Background Service)**:

```csharp
// Force API key authentication (no user context)
var volunteers = await ApiClient.GetOnlineVolunteersAsync();
```

**API receives**:
```http
GET /api/volunteers/online HTTP/1.1
Host: api.aidcircle.net
X-API-Key: your-secure-api-key-32-chars-min
```

---

## API Middleware Logic

The `DualAuthenticationMiddleware` checks for authentication in this order:

1. **Check for Bearer token** (Azure AD B2C)
   - If present → validate via Azure AD
   - If valid → allow request

2. **Check for API key** (X-API-Key header)
   - If present → validate against Key Vault configuration
   - If valid → allow request

3. **Reject request** if neither authentication method is valid

**Exempt endpoints** (no authentication required):
- `/health`
- `/swagger`
- `/_framework`

---

## Security Best Practices

### ✅ DO

- **Use managed identities** for Azure resource authentication (no credentials in code)
- **Rotate API keys** regularly (every 90 days recommended)
- **Use HTTPS only** for all API communication
- **Store all secrets in Key Vault** - never in appsettings.json or code
- **Use separate API keys** for dev/staging/production environments
- **Grant least privilege** - only necessary Key Vault permissions
- **Enable Key Vault logging** to audit secret access
- **Use Azure AD B2C tokens** for user operations (audit trail)

### ❌ DON'T

- **Never commit secrets** to version control (use `.gitignore`)
- **Don't share API keys** across environments
- **Don't use API keys for user operations** (use B2C tokens)
- **Don't expose API key** in client-side code or URLs
- **Don't disable HTTPS** in production
- **Don't skip CORS configuration** (security vulnerability)

---

## Production Configuration

### Web App (`appsettings.json`)

```json
{
  "KeyVaultName": "aidcirclekeyvault",
  "ApiSettings": {
    "BaseUrl": "https://api.aidcircle.net"
  },
  "AzureAd": {
    "Instance": "https://aidcircle.b2clogin.com/",
    "Domain": "aidcircle.onmicrosoft.com",
    "ClientId": "your-client-id",
    "TenantId": "your-tenant-id",
    "CallbackPath": "/signin-oidc",
    "SignedOutCallbackPath": "/signout-callback-oidc"
  },
  "AzureOpenAI": {
    "Endpoint": "https://your-resource.openai.azure.com/",
    "DeploymentName": "gpt-4o-mini"
  },
  "AzureTranslator": {
    "Endpoint": "https://api.cognitive.microsofttranslator.com",
    "Region": "eastus"
  }
}
```

**Note**: Secrets (`ApiKey`, `ClientSecret`, connection strings) loaded from Key Vault.

### API App (`appsettings.json`)

```json
{
  "KeyVaultName": "aidcirclekeyvault",
  "ApiSettings": {
    "AllowedOrigins": "https://aidcircle.net,https://www.aidcircle.net"
  },
  "AzureOpenAI": {
    "Endpoint": "https://your-resource.openai.azure.com/",
    "DeploymentName": "gpt-4o-mini"
  },
  "AzureTranslator": {
    "Endpoint": "https://api.cognitive.microsofttranslator.com",
    "Region": "eastus"
  }
}
```

---

## Troubleshooting

### Issue: "Unauthorized: Missing authentication credentials"

**Cause**: Neither B2C token nor API key present in request.

**Solution**:
1. For user operations: Ensure user is logged in via Azure AD B2C
2. For service operations: Add API key to configuration:
   ```json
   "ApiSettings": {
     "ApiKey": "your-api-key-from-keyvault"
   }
   ```

### Issue: "Invalid API key"

**Cause**: API key doesn't match value in Key Vault.

**Solution**:
```powershell
# Retrieve current API key from Key Vault
az keyvault secret show \
  --vault-name aidcirclekeyvault \
  --name "ApiSettings--ApiKey" \
  --query value -o tsv
```

Update your local configuration file with the correct key.

### Issue: "403 Forbidden" when accessing Key Vault

**Cause**: Managed identity or user doesn't have Key Vault access.

**Solution**:
```powershell
# Grant access to your Azure AD user
az keyvault set-policy \
  --name aidcirclekeyvault \
  --upn your-email@domain.com \
  --secret-permissions get list

# Or for managed identity
az keyvault set-policy \
  --name aidcirclekeyvault \
  --object-id <principal-id> \
  --secret-permissions get list
```

### Issue: CORS error in browser

**Cause**: Web app origin not in API's allowed origins list.

**Solution**:
```powershell
# Update API appsettings.json
az keyvault secret set \
  --vault-name aidcirclekeyvault \
  --name "ApiSettings--AllowedOrigins" \
  --value "https://aidcircle.net,https://www.aidcircle.net,https://localhost:5011"
```

---

## Monitoring & Auditing

### Enable Key Vault Logging

```powershell
# Create Log Analytics workspace
az monitor log-analytics workspace create \
  --resource-group rg-aidcircle-prod \
  --workspace-name aidcircle-logs

# Get workspace ID
$workspaceId = az monitor log-analytics workspace show \
  --resource-group rg-aidcircle-prod \
  --workspace-name aidcircle-logs \
  --query id -o tsv

# Enable diagnostics for Key Vault
az monitor diagnostic-settings create \
  --name KeyVaultAudit \
  --resource /subscriptions/YOUR_SUB/resourceGroups/rg-aidcircle-prod/providers/Microsoft.KeyVault/vaults/aidcirclekeyvault \
  --workspace $workspaceId \
  --logs '[{"category": "AuditEvent", "enabled": true}]'
```

### Query Key Vault Access Logs

```kql
AzureDiagnostics
| where ResourceProvider == "MICROSOFT.KEYVAULT"
| where OperationName == "SecretGet"
| project TimeGenerated, CallerIPAddress, identity_claim_appid_g, requestUri_s
| order by TimeGenerated desc
```

---

## Next Steps

- 📚 **Understand the Architecture** → [Architecture Overview](architecture-overview.md)
- 💻 **Local Development** → [Local Development Guide](local-development.md)
- ☁️ **Deploy to Azure** → [Azure App Service Deployment](deployment.md)
- 🖥️ **Deploy to IIS** → [IIS + Azure Arc Deployment](deployment-iis-arc.md)
