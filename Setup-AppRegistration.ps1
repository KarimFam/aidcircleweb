# Quick Setup - App Registration with Client Secret (Fallback for Managed Identity)
# This allows local development AND production Arc managed identity with same code

Write-Host "=== Azure AD App Registration Setup ===" -ForegroundColor Cyan
Write-Host ""

# Step 1: Create or get existing app registration
Write-Host "[1/4] Setting up App Registration..." -ForegroundColor Yellow
$appName = "aidcircle-app"

$existingApp = az ad app list --display-name $appName --query "[0]" 2>$null | ConvertFrom-Json

if ($existingApp) {
    Write-Host "   Found existing app: $appName" -ForegroundColor Green
    $appId = $existingApp.appId
} else {
    Write-Host "   Creating new app registration..." -ForegroundColor Gray
    $newApp = az ad app create --display-name $appName --query "{appId:appId}" -o json | ConvertFrom-Json
    $appId = $newApp.appId
    
    # Create service principal
    az ad sp create --id $appId --output none
    Write-Host "   ✅ App created: $appName" -ForegroundColor Green
}

Write-Host "   App ID (Client ID): $appId" -ForegroundColor Cyan
Write-Host ""

# Step 2: Create new client secret
Write-Host "[2/4] Creating Client Secret..." -ForegroundColor Yellow
$secretResult = az ad app credential reset --id $appId --query "password" -o tsv

Write-Host "   ✅ Client Secret created" -ForegroundColor Green
Write-Host ""
Write-Host "   🔐 SAVE THIS SECRET - IT WON'T BE SHOWN AGAIN:" -ForegroundColor Red
Write-Host "   $secretResult" -ForegroundColor Yellow
Write-Host ""

# Step 3: Get tenant ID
$tenantId = az account show --query tenantId -o tsv
Write-Host "   Tenant ID: $tenantId" -ForegroundColor Gray
Write-Host ""

# Step 4: Grant Key Vault access
Write-Host "[3/4] Granting Key Vault Access..." -ForegroundColor Yellow

$spObjectId = az ad sp list --display-name $appName --query "[0].id" -o tsv
$kvResourceId = az keyvault show --name "aidcirclekeyvault" --query "id" -o tsv

# Check if role already assigned
$existingRole = az role assignment list --assignee $spObjectId --scope $kvResourceId --query "[?roleDefinitionName=='Key Vault Secrets User'].id" -o tsv 2>$null

if (![string]::IsNullOrEmpty($existingRole)) {
    Write-Host "   ✅ Key Vault Secrets User role already assigned" -ForegroundColor Green
} else {
    az role assignment create --role "Key Vault Secrets User" --assignee $spObjectId --scope $kvResourceId --output none
    Write-Host "   ✅ Key Vault Secrets User role assigned" -ForegroundColor Green
}
Write-Host ""

# Step 5: Show configuration
Write-Host "[4/4] Configuration Values" -ForegroundColor Yellow
Write-Host ""
Write-Host "=== FOR LOCAL DEVELOPMENT (appsettings.Development.Local.json) ===" -ForegroundColor Cyan
Write-Host @"
{
  "AZURE_TENANT_ID": "$tenantId",
  "AZURE_CLIENT_ID": "$appId",
  "AZURE_CLIENT_SECRET": "$secretResult"
}
"@
Write-Host ""

Write-Host "=== FOR PRODUCTION WEB1 (web.config) ===" -ForegroundColor Cyan
Write-Host @"
<environmentVariables>
  <environmentVariable name="AZURE_TENANT_ID" value="$tenantId" />
  <environmentVariable name="AZURE_CLIENT_ID" value="$appId" />
  <environmentVariable name="AZURE_CLIENT_SECRET" value="$secretResult" />
  <environmentVariable name="KeyVaultName" value="aidcirclekeyvault" />
  <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
</environmentVariables>
"@
Write-Host ""

Write-Host "=== IMPORTANT ===" -ForegroundColor Red
Write-Host "Your current code in Program.cs already supports BOTH:" -ForegroundColor Yellow
Write-Host "  1. Client ID + Secret (reads AZURE_CLIENT_SECRET from environment)" -ForegroundColor White
Write-Host "  2. Managed Identity (uses AZURE_CLIENT_ID if no secret present)" -ForegroundColor White
Write-Host ""
Write-Host "On WEB1 with Arc agent:" -ForegroundColor Cyan
Write-Host "  - Use Arc managed identity (remove AZURE_CLIENT_SECRET from web.config)" -ForegroundColor White
Write-Host "  - Keep only AZURE_CLIENT_ID = cc261686-88bf-4252-84c1-44b28dd1c533" -ForegroundColor White
Write-Host ""
Write-Host "For local dev or servers WITHOUT Arc:" -ForegroundColor Cyan
Write-Host "  - Add all three variables (TENANT_ID, CLIENT_ID, CLIENT_SECRET)" -ForegroundColor White
Write-Host ""
