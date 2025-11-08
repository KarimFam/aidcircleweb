# Azure Key Vault Setup Instructions

This project uses Azure Key Vault to securely store sensitive configuration values like API keys, connection strings, and secrets.

## Required Secrets in Key Vault

Create the following secrets in your Azure Key Vault named `aidcirclekeyvault`:

### Database Connection
- **Secret Name**: `ConnectionStrings--H4HDB-DEV`
- **Value**: Your SQL Server connection string
- **Example**: `Server=tcp:h4hdevkfam.database.windows.net,1433;Initial Catalog=h4h-dbserver-dev;Persist Security Info=False;User ID=serveradmin;Password=YOUR_PASSWORD;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;`

### Azure OpenAI API Key
- **Secret Name**: `AzureOpenAI--ApiKey`
- **Value**: Your Azure OpenAI API key

### Azure Translator Key
- **Secret Name**: `AzureTranslator--Key`
- **Value**: Your Azure Translator service key

## Key Vault Naming Convention

Azure Key Vault uses `--` (double dash) to represent `:` (colon) in hierarchical configuration keys.

Examples:
- `ConnectionStrings:H4HDB-DEV` → `ConnectionStrings--H4HDB-DEV`
- `AzureOpenAI:ApiKey` → `AzureOpenAI--ApiKey`
- `AzureTranslator:Key` → `AzureTranslator--Key`

## Azure CLI Commands to Create Secrets

```bash
# Login to Azure
az login

# Set your subscription
az subscription set --subscription "YOUR_SUBSCRIPTION_ID"

# Create Key Vault (if it doesn't exist)
az keyvault create --name aidcirclekeyvault --resource-group YOUR_RESOURCE_GROUP --location eastus2

# Add Database Connection String
az keyvault secret set --vault-name aidcirclekeyvault --name "ConnectionStrings--H4HDB-DEV" --value "YOUR_CONNECTION_STRING"

# Add Azure OpenAI API Key
az keyvault secret set --vault-name aidcirclekeyvault --name "AzureOpenAI--ApiKey" --value "YOUR_OPENAI_KEY"

# Add Azure Translator Key
az keyvault secret set --vault-name aidcirclekeyvault --name "AzureTranslator--Key" --value "YOUR_TRANSLATOR_KEY"
```

## Local Development Setup

### ⭐ Option 1: appsettings.Development.Local.json (Recommended for Team)

**This is the easiest and most common approach for local development.**

1. Copy the template files:
   ```powershell
   # For API
   Copy-Item "H4H.Presentation.API\appsettings.Development.Local.json.template" "H4H.Presentation.API\appsettings.Development.Local.json"
   
   # For Web
   Copy-Item "H4H.Presentation.Web\H4H.Presentation.Web\appsettings.Development.Local.json.template" "H4H.Presentation.Web\H4H.Presentation.Web\appsettings.Development.Local.json"
   ```

2. Edit the files and replace placeholders with your actual secrets

3. **These files are git-ignored** - they will never be committed to source control

See [LOCAL-DEVELOPMENT-SETUP.md](./LOCAL-DEVELOPMENT-SETUP.md) for complete instructions.

### Option 2: Azure CLI Authentication (for Key Vault Testing)

If you want to test Key Vault integration locally instead of using local files:

1. Install Azure CLI: https://docs.microsoft.com/cli/azure/install-azure-cli
2. Login to Azure: `az login`
3. The application will automatically use your Azure CLI credentials via `DefaultAzureCredential`
4. Ensure your Azure account has "Key Vault Secrets User" role on the vault

### Option 3: Visual Studio Authentication

For developers using Visual Studio:

1. Sign in to Visual Studio with your Azure account
2. Go to Tools → Options → Azure Service Authentication
3. Select your account
4. `DefaultAzureCredential` will use your VS credentials

### Option 4: User Secrets (Alternative to Local Files)

If you don't want to use Key Vault locally, you can use .NET User Secrets:

```bash
# Navigate to Web project
cd H4H.Presentation.Web/H4H.Presentation.Web

# Initialize user secrets
dotnet user-secrets init

# Set secrets
dotnet user-secrets set "ConnectionStrings:H4HDB-DEV" "YOUR_CONNECTION_STRING"
dotnet user-secrets set "AzureOpenAI:ApiKey" "YOUR_OPENAI_KEY"
dotnet user-secrets set "AzureTranslator:Key" "YOUR_TRANSLATOR_KEY"

# Same for API project
cd ../../H4H.Presentation.API
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:H4HDB-DEV" "YOUR_CONNECTION_STRING"
dotnet user-secrets set "AzureOpenAI:ApiKey" "YOUR_OPENAI_KEY"
dotnet user-secrets set "AzureTranslator:Key" "YOUR_TRANSLATOR_KEY"
```

## Azure Deployment (Production)

### Assign Managed Identity Access

When deploying to Azure App Service, enable Managed Identity and grant Key Vault access:

```bash
# Enable System-assigned Managed Identity on App Service
az webapp identity assign --name YOUR_APP_NAME --resource-group YOUR_RESOURCE_GROUP

# Get the principal ID (output from previous command)
PRINCIPAL_ID=$(az webapp identity show --name YOUR_APP_NAME --resource-group YOUR_RESOURCE_GROUP --query principalId -o tsv)

# Grant Key Vault access
az keyvault set-policy --name aidcirclekeyvault --object-id $PRINCIPAL_ID --secret-permissions get list
```

## Verify Configuration

Run the application and check that it successfully reads from Key Vault:

```bash
dotnet run --project H4H.Presentation.Web/H4H.Presentation.Web
```

If successful, you should see no configuration errors and the application should connect to the database and Azure services.

## Troubleshooting

### Error: "No such host is known" for Key Vault
- Ensure you're logged in with `az login`
- Check your internet connection
- Verify Key Vault name is correct in `appsettings.json`

### Error: "Access denied" or "Forbidden"
- Ensure your Azure account has "Get" and "List" permissions on Key Vault secrets
- Run: `az keyvault set-policy --name aidcirclekeyvault --upn YOUR_EMAIL --secret-permissions get list`

### Error: "Secret not found"
- Verify secret names use `--` instead of `:`
- Check secret exists: `az keyvault secret list --vault-name aidcirclekeyvault`

## Security Best Practices

1. **Never commit secrets to Git** - Key Vault ensures secrets stay out of source control
2. **Use Managed Identities in Azure** - No need for connection strings or keys in production
3. **Rotate secrets regularly** - Update Key Vault secrets, app automatically picks up changes
4. **Limit Key Vault access** - Grant least-privilege access via Azure RBAC
5. **Enable Key Vault logging** - Track who accesses secrets and when

## Configuration Hierarchy

.NET reads configuration in this order (later sources override earlier):
1. `appsettings.json`
2. `appsettings.{Environment}.json`
3. User Secrets (Development only)
4. Environment Variables
5. **Azure Key Vault** (added via this setup)
6. Command-line arguments

This means Key Vault values will override appsettings.json values!
