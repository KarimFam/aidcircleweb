# Configure-IISArcIdentity.ps1
# Automates Azure Arc Managed Identity configuration for IIS applications

param(
    [Parameter(Mandatory=$false)]
    [string]$MachineName,
    
    [Parameter(Mandatory=$false)]
    [string]$ArcResourceGroup,
    
    [Parameter(Mandatory=$false)]
    [string]$ArcSubscription,
    
    [Parameter(Mandatory=$false)]
    [string]$WebAppPath = "E:\aidcircle\web",
    
    [Parameter(Mandatory=$false)]
    [string]$ApiAppPath = "E:\aidcircle\api",
    
    [Parameter(Mandatory=$false)]
    [string]$WebAppPoolName,
    
    [Parameter(Mandatory=$false)]
    [string]$ApiAppPoolName,
    
    [Parameter(Mandatory=$false)]
    [string]$KeyVaultName,
    
    [Parameter(Mandatory=$false)]
    [string]$KeyVaultSubscription,
    
    [Parameter(Mandatory=$false)]
    [string]$AzureOpenAIEndpoint,
    
    [Parameter(Mandatory=$false)]
    [string]$AzureOpenAIDeployment = "gpt-4o-mini"
)


Write-Host "========================================" -ForegroundColor Cyan
Write-Host "IIS Azure Arc Identity Configuration" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Collect missing parameters interactively
if ([string]::IsNullOrEmpty($MachineName)) {
    $MachineName = Read-Host "Enter the Azure Arc machine name (hostname of IIS server)"
}

if ([string]::IsNullOrEmpty($ArcResourceGroup)) {
    Write-Host ""
    Write-Host "ℹ️  This is the resource group where your Azure Arc-enabled server is registered in Azure." -ForegroundColor Cyan
    $ArcResourceGroup = Read-Host "Enter the Azure Arc resource group name"
}

if ([string]::IsNullOrEmpty($ArcSubscription)) {
    Write-Host ""
    Write-Host "ℹ️  If your Arc machine is in a different subscription than the current one, enter it here." -ForegroundColor Cyan
    Write-Host "   Otherwise, press Enter to search current subscription first." -ForegroundColor Cyan
    $ArcSubscription = Read-Host "Enter Arc machine subscription name or ID (optional)"
}

if ([string]::IsNullOrEmpty($KeyVaultName)) {
    Write-Host ""
    Write-Host "ℹ️  Enter your Azure Key Vault name (e.g., kv-aidcircle-prod, aidcirclekeyvault)" -ForegroundColor Cyan
    $KeyVaultName = Read-Host "Enter the Key Vault name"
}

if ([string]::IsNullOrEmpty($KeyVaultSubscription)) {
    Write-Host ""
    Write-Host "ℹ️  If your Key Vault is in a different subscription than your Arc machine, enter it here." -ForegroundColor Cyan
    Write-Host "   Otherwise, press Enter to search current subscription first." -ForegroundColor Cyan
    $KeyVaultSubscription = Read-Host "Enter Key Vault subscription name or ID (optional)"
}

if ([string]::IsNullOrEmpty($AzureOpenAIEndpoint)) {
    Write-Host ""
    Write-Host "ℹ️  Enter your Azure OpenAI endpoint (e.g., https://your-resource.openai.azure.com/)" -ForegroundColor Cyan
    $AzureOpenAIEndpoint = Read-Host "Enter the Azure OpenAI endpoint"
}

if ([string]::IsNullOrEmpty($WebAppPoolName)) {
    Write-Host ""
    $WebAppPoolName = Read-Host "Enter the IIS App Pool name for the Web application (default: AidCircleWebPool)"
    if ([string]::IsNullOrEmpty($WebAppPoolName)) {
        $WebAppPoolName = "AidCircleWebPool"
    }
}

if ([string]::IsNullOrEmpty($ApiAppPoolName)) {
    Write-Host ""
    $ApiAppPoolName = Read-Host "Enter the IIS App Pool name for the API application (default: AidCircleApiPool)"
    if ([string]::IsNullOrEmpty($ApiAppPoolName)) {
        $ApiAppPoolName = "AidCircleApiPool"
    }
}

Write-Host ""
Write-Host "Configuration Summary:" -ForegroundColor Yellow
Write-Host "  Arc Machine Name: $MachineName" -ForegroundColor White
Write-Host "  Arc Resource Group: $ArcResourceGroup" -ForegroundColor White
Write-Host "  Key Vault: $KeyVaultName" -ForegroundColor White
Write-Host "  Web App Path: $WebAppPath" -ForegroundColor White
Write-Host "  API App Path: $ApiAppPath" -ForegroundColor White
Write-Host "  Web App Pool: $WebAppPoolName" -ForegroundColor White
Write-Host "  API App Pool: $ApiAppPoolName" -ForegroundColor White
Write-Host ""

$confirm = Read-Host "Proceed with configuration? (Y/N)"
if ($confirm -ne 'Y' -and $confirm -ne 'y') {
    Write-Host "Configuration cancelled." -ForegroundColor Yellow
    exit 0
}
Write-Host ""

# Step 1: Check if Azure CLI is installed
Write-Host "[1/7] Checking Azure CLI..." -ForegroundColor Yellow
try {
    $azVersion = az version --query '\"azure-cli\"' -o tsv
    Write-Host "✅ Azure CLI version $azVersion found" -ForegroundColor Green
} catch {
    Write-Host "❌ Azure CLI not found. Install from: https://aka.ms/installazurecliwindows" -ForegroundColor Red
    exit 1
}

# Step 2: Check if logged into Azure
Write-Host "[2/7] Checking Azure login..." -ForegroundColor Yellow
try {
    $account = az account show --query name -o tsv 2>$null
    if ($null -eq $account) {
        Write-Host "❌ Not logged into Azure. Run: az login" -ForegroundColor Red
        exit 1
    }
    Write-Host "✅ Logged in as: $account" -ForegroundColor Green
} catch {
    Write-Host "❌ Not logged into Azure. Run: az login" -ForegroundColor Red
    exit 1
}

# Step 3: Get Arc Managed Identity Principal ID
Write-Host "[3/7] Retrieving Arc Managed Identity..." -ForegroundColor Yellow
try {
    # Get current subscription
    $currentSub = az account show --query name -o tsv
    $currentSubId = az account show --query id -o tsv
    Write-Host "   Current subscription: $currentSub" -ForegroundColor Gray
    
    $principalId = $null
    $arcMachineFound = $false
    
    # If ArcSubscription is provided, switch to it first
    if (![string]::IsNullOrEmpty($ArcSubscription)) {
        Write-Host "   Switching to Arc machine subscription: $ArcSubscription" -ForegroundColor Cyan
        az account set --subscription $ArcSubscription 2>$null
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "⚠️  Could not switch to subscription '$ArcSubscription'. Searching all subscriptions..." -ForegroundColor Yellow
            az account set --subscription $currentSubId 2>$null
            $ArcSubscription = $null
        } else {
            # Try to find Arc machine in specified subscription
            $principalId = az connectedmachine show `
                --name $MachineName `
                --resource-group $ArcResourceGroup `
                --query "identity.principalId" -o tsv 2>$null
            
            if ($null -ne $principalId -and $principalId -ne "") {
                $arcMachineFound = $true
                Write-Host "   ✅ Found Arc machine in specified subscription" -ForegroundColor Green
            }
        }
    }
    
    # If not found yet, try current subscription
    if (!$arcMachineFound) {
        az account set --subscription $currentSubId 2>$null
        
        $principalId = az connectedmachine show `
            --name $MachineName `
            --resource-group $ArcResourceGroup `
            --query "identity.principalId" -o tsv 2>$null
        
        if ($null -ne $principalId -and $principalId -ne "") {
            $arcMachineFound = $true
            Write-Host "   ✅ Found Arc machine in current subscription" -ForegroundColor Green
        }
    }
    
    # If still not found and no subscription was specified, search all subscriptions
    if (!$arcMachineFound -and [string]::IsNullOrEmpty($ArcSubscription)) {
        Write-Host "⚠️  Arc machine '$MachineName' not found in current subscription." -ForegroundColor Yellow
        Write-Host ""
        Write-Host "Searching across all subscriptions..." -ForegroundColor Cyan
        
        $allSubs = az account list --query "[].{Name:name, Id:id}" -o json | ConvertFrom-Json
        
        foreach ($sub in $allSubs) {
            Write-Host "   Checking subscription: $($sub.Name)..." -ForegroundColor Gray
            az account set --subscription $sub.Id 2>$null
            
            $principalIdCheck = az connectedmachine show `
                --name $MachineName `
                --resource-group $ArcResourceGroup `
                --query "identity.principalId" -o tsv 2>$null
            
            if ($null -ne $principalIdCheck -and $principalIdCheck -ne "") {
                $principalId = $principalIdCheck
                $ArcSubscription = $sub.Id
                $arcMachineFound = $true
                Write-Host "   ✅ Found Arc machine in subscription: $($sub.Name)" -ForegroundColor Green
                break
            }
        }
        
        # Restore original subscription
        az account set --subscription $currentSubId 2>$null
    }
    
    if (!$arcMachineFound -or $null -eq $principalId -or $principalId -eq "") {
        Write-Host ""
        Write-Host "❌ Could not retrieve managed identity. Arc machine not found." -ForegroundColor Red
        Write-Host ""
        Write-Host "Troubleshooting steps:" -ForegroundColor Yellow
        Write-Host "   1. Run 'azcmagent show' on server '$MachineName' to verify Arc agent is connected" -ForegroundColor Gray
        Write-Host "   2. Verify machine name '$MachineName' is correct" -ForegroundColor Gray
        Write-Host "   3. Verify resource group '$ArcResourceGroup' is correct" -ForegroundColor Gray
        Write-Host "   4. Check if machine is in a different subscription" -ForegroundColor Gray
        Write-Host ""
        Write-Host "Available Arc machines across all subscriptions:" -ForegroundColor Yellow
        
        $allSubs = az account list --query "[].{Name:name, Id:id}" -o json | ConvertFrom-Json
        foreach ($sub in $allSubs) {
            az account set --subscription $sub.Id 2>$null
            $machines = az connectedmachine list --query "[].{Name:name, ResourceGroup:resourceGroup, Subscription:'$($sub.Name)'}" -o table 2>$null
            if (![string]::IsNullOrEmpty($machines)) {
                Write-Host ""
                Write-Host "Subscription: $($sub.Name)" -ForegroundColor Cyan
                Write-Host $machines -ForegroundColor Gray
            }
        }
        
        # Restore original subscription
        az account set --subscription $currentSubId 2>$null
        exit 1
    }
    
    Write-Host "✅ Managed Identity Principal ID: $principalId" -ForegroundColor Green
} catch {
    Write-Host "❌ Error retrieving managed identity: $_" -ForegroundColor Red
    exit 1
}

# Step 4: Grant Key Vault Access
Write-Host "[4/7] Granting Key Vault access..." -ForegroundColor Yellow
try {
    # Store the subscription where Arc machine was found (might be different from current)
    $arcMachineSubId = az account show --query id -o tsv
    $arcMachineSubName = az account show --query name -o tsv
    Write-Host "   Arc machine subscription: $arcMachineSubName" -ForegroundColor Gray
    
    $kvFoundInSub = $null
    
    # If KeyVaultSubscription is provided, switch to it first
    if (![string]::IsNullOrEmpty($KeyVaultSubscription)) {
        Write-Host "   Switching to Key Vault subscription: $KeyVaultSubscription" -ForegroundColor Cyan
        az account set --subscription $KeyVaultSubscription 2>$null
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "⚠️  Could not switch to subscription '$KeyVaultSubscription'. Searching all subscriptions..." -ForegroundColor Yellow
            az account set --subscription $arcMachineSubId 2>$null
            $KeyVaultSubscription = $null
        } else {
            $kvFoundInSub = $KeyVaultSubscription
        }
    }
    
    # Try to find Key Vault in current context
    $kvExists = az keyvault show --name $KeyVaultName --query "name" -o tsv 2>$null
    
    # If not found and no subscription was specified, search all subscriptions
    if (($null -eq $kvExists -or $kvExists -eq "") -and [string]::IsNullOrEmpty($KeyVaultSubscription)) {
        Write-Host "⚠️  Key Vault '$KeyVaultName' not found in current subscription." -ForegroundColor Yellow
        Write-Host ""
        Write-Host "Searching across all subscriptions..." -ForegroundColor Cyan
        
        # Search all subscriptions
        $allSubs = az account list --query "[].{Name:name, Id:id}" -o json | ConvertFrom-Json
        $foundKV = $null
        $foundSubId = $null
        
        foreach ($sub in $allSubs) {
            Write-Host "   Checking subscription: $($sub.Name)..." -ForegroundColor Gray
            az account set --subscription $sub.Id 2>$null
            
            $kvCheck = az keyvault show --name $KeyVaultName --query "{name:name, id:id}" -o json 2>$null
            if ($null -ne $kvCheck -and $kvCheck -ne "") {
                $foundKV = $kvCheck | ConvertFrom-Json
                $foundSubId = $sub.Id
                $kvFoundInSub = $sub.Id
                Write-Host "   ✅ Found Key Vault in subscription: $($sub.Name)" -ForegroundColor Green
                break
            }
        }
        
        if ($null -eq $foundKV) {
            Write-Host ""
            Write-Host "❌ Key Vault '$KeyVaultName' not found in any subscription you have access to." -ForegroundColor Red
            Write-Host ""
            Write-Host "Available Key Vaults across all subscriptions:" -ForegroundColor Yellow
            
            foreach ($sub in $allSubs) {
                az account set --subscription $sub.Id 2>$null
                $vaults = az keyvault list --query "[].{Name:name, Subscription:'$($sub.Name)'}" -o table 2>$null
                if (![string]::IsNullOrEmpty($vaults)) {
                    Write-Host ""
                    Write-Host "Subscription: $($sub.Name)" -ForegroundColor Cyan
                    Write-Host $vaults -ForegroundColor Gray
                }
            }
            
            # Restore Arc machine subscription
            az account set --subscription $arcMachineSubId 2>$null
            
            Write-Host ""
            $newVaultName = Read-Host "Enter correct Key Vault name (or press Enter to skip)"
            if (![string]::IsNullOrEmpty($newVaultName)) {
                $KeyVaultName = $newVaultName
                throw "Retry"
            } else {
                Write-Host "⚠️  Skipping Key Vault access grant. Configure manually later." -ForegroundColor Yellow
                throw "Skipped"
            }
        } else {
            $KeyVaultName = $foundKV.name
        }
    } else {
        Write-Host "   ✅ Key Vault found in current subscription context" -ForegroundColor Green
        $kvFoundInSub = az account show --query id -o tsv
    }
    
    # Grant RBAC role (Key Vaults with --enable-rbac-authorization use RBAC, not access policies)
    Write-Host "   Granting Key Vault Secrets User role to Principal ID: $principalId" -ForegroundColor Gray
    
    # Get Key Vault resource ID for role assignment scope
    $kvResourceId = az keyvault show --name $KeyVaultName --query "id" -o tsv 2>$null
    
    # Check if role already assigned
    $existingRole = az role assignment list --assignee $principalId --scope $kvResourceId --query "[?roleDefinitionName=='Key Vault Secrets User'].id" -o tsv 2>$null
    
    if (![string]::IsNullOrEmpty($existingRole)) {
        Write-Host "✅ Key Vault Secrets User role already assigned" -ForegroundColor Green
    } else {
        # Assign role
        az role assignment create `
            --role "Key Vault Secrets User" `
            --assignee $principalId `
            --scope $kvResourceId `
            --output none 2>$null
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Key Vault Secrets User role assigned" -ForegroundColor Green
        } else {
            Write-Host "❌ Failed to assign Key Vault role" -ForegroundColor Red
            Write-Host "   This might be a permissions issue. Try running:" -ForegroundColor Yellow
            Write-Host "   az account set --subscription '$kvFoundInSub'" -ForegroundColor Gray
            Write-Host "   az role assignment create --role 'Key Vault Secrets User' --assignee $principalId --scope $kvResourceId" -ForegroundColor Gray
        }
    }
    
    # Always restore to Arc machine subscription for remaining steps
    Write-Host "   Restoring to Arc machine subscription..." -ForegroundColor Gray
    az account set --subscription $arcMachineSubId 2>$null
    
} catch {
    # Always try to restore Arc machine subscription
    if (![string]::IsNullOrEmpty($arcMachineSubId)) {
        az account set --subscription $arcMachineSubId 2>$null
    }
    
    if ($_.Exception.Message -eq "Retry") {
        Write-Host "⚠️  Please run the script again with the correct Key Vault name." -ForegroundColor Yellow
    } elseif ($_.Exception.Message -ne "Skipped") {
        Write-Host "⚠️  Could not grant Key Vault access. You may need to do this manually:" -ForegroundColor Yellow
        Write-Host "   az account set --subscription 'KEYVAULT_SUBSCRIPTION'" -ForegroundColor Gray
        Write-Host "   az role assignment create --role 'Key Vault Secrets User' --assignee $principalId --scope /subscriptions/.../resourceGroups/.../providers/Microsoft.KeyVault/vaults/$KeyVaultName" -ForegroundColor Gray
    }
}

# Step 5: Update Web App web.config
Write-Host "[5/7] Updating Web app web.config..." -ForegroundColor Yellow
$webConfigPath = Join-Path $WebAppPath "web.config"

if (Test-Path $webConfigPath) {
    try {
        [xml]$webConfig = Get-Content $webConfigPath
        
        # Navigate to aspNetCore element - handle both single site and location-wrapped configs
        $aspNetCore = $null
        if ($webConfig.configuration.location) {
            $aspNetCore = $webConfig.configuration.location.'system.webServer'.aspNetCore
        } else {
            $aspNetCore = $webConfig.configuration.'system.webServer'.aspNetCore
        }
        
        if ($null -eq $aspNetCore) {
            Write-Host "❌ Could not find aspNetCore element in web.config" -ForegroundColor Red
            Write-Host "   Please update manually using IIS-ENVIRONMENT-VARIABLES-GUIDE.md" -ForegroundColor Yellow
            throw "Missing aspNetCore element"
        }
        
        # Find or create environmentVariables section
        $envVarsNode = $aspNetCore.SelectSingleNode("environmentVariables")
        
        if ($null -eq $envVarsNode) {
            $envVarsNode = $webConfig.CreateElement("environmentVariables")
            [void]$aspNetCore.AppendChild($envVarsNode)
        }
        
        # Helper function to add/update environment variable
        function Set-EnvVar {
            param([xml]$doc, [System.Xml.XmlElement]$parent, [string]$name, [string]$value)
            
            $existing = $parent.SelectSingleNode("environmentVariable[@name='$name']")
            if ($null -ne $existing) {
                $existing.SetAttribute("value", $value)
            } else {
                $newVar = $doc.CreateElement("environmentVariable")
                $newVar.SetAttribute("name", $name)
                $newVar.SetAttribute("value", $value)
                [void]$parent.AppendChild($newVar)
            }
        }
        
        # Set critical variables
        Set-EnvVar $webConfig $envVarsNode "AZURE_CLIENT_ID" $principalId
        Set-EnvVar $webConfig $envVarsNode "KeyVaultName" $KeyVaultName
        Set-EnvVar $webConfig $envVarsNode "ASPNETCORE_ENVIRONMENT" "Production"
        Set-EnvVar $webConfig $envVarsNode "AzureOpenAI__Endpoint" $AzureOpenAIEndpoint
        Set-EnvVar $webConfig $envVarsNode "AzureOpenAI__DeploymentName" $AzureOpenAIDeployment
        Set-EnvVar $webConfig $envVarsNode "AzureTranslator__Endpoint" "https://api.cognitive.microsofttranslator.com"
        Set-EnvVar $webConfig $envVarsNode "AzureTranslator__Region" "eastus"
        Set-EnvVar $webConfig $envVarsNode "AzureAdB2C__Instance" "https://aidcirclenet.b2clogin.com/"
        Set-EnvVar $webConfig $envVarsNode "AzureAdB2C__Domain" "aidcirclenet.onmicrosoft.com"
        Set-EnvVar $webConfig $envVarsNode "AzureAdB2C__ClientId" "36bf28fa-d15c-4762-8c32-8e46c3aa9051"
        Set-EnvVar $webConfig $envVarsNode "AzureAdB2C__SignUpSignInPolicyId" "B2C_1_susi"
        Set-EnvVar $webConfig $envVarsNode "ApiSettings__BaseUrl" "http://localhost:5135"
        
        # Save backup
        $backupPath = "$webConfigPath.backup_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
        Copy-Item $webConfigPath $backupPath -Force
        
        # Save updated config
        $webConfig.Save($webConfigPath)
        Write-Host "✅ Web app web.config updated (backup: $backupPath)" -ForegroundColor Green
    } catch {
        Write-Host "❌ Error updating web.config: $_" -ForegroundColor Red
        Write-Host "   Please update manually using IIS-ENVIRONMENT-VARIABLES-GUIDE.md" -ForegroundColor Yellow
    }
} else {
    Write-Host "⚠️  web.config not found at: $webConfigPath" -ForegroundColor Yellow
    Write-Host "   Please ensure app is deployed first." -ForegroundColor Gray
}

# Step 6: Update API App web.config
Write-Host "[6/7] Updating API app web.config..." -ForegroundColor Yellow
$apiConfigPath = Join-Path $ApiAppPath "web.config"

if (Test-Path $apiConfigPath) {
    try {
        [xml]$apiConfig = Get-Content $apiConfigPath
        
        # Navigate to aspNetCore element - handle both single site and location-wrapped configs
        $aspNetCore = $null
        if ($apiConfig.configuration.location) {
            $aspNetCore = $apiConfig.configuration.location.'system.webServer'.aspNetCore
        } else {
            $aspNetCore = $apiConfig.configuration.'system.webServer'.aspNetCore
        }
        
        if ($null -eq $aspNetCore) {
            Write-Host "❌ Could not find aspNetCore element in web.config" -ForegroundColor Red
            throw "Missing aspNetCore element"
        }
        
        # Find or create environmentVariables section
        $envVarsNode = $aspNetCore.SelectSingleNode("environmentVariables")
        
        if ($null -eq $envVarsNode) {
            $envVarsNode = $apiConfig.CreateElement("environmentVariables")
            [void]$aspNetCore.AppendChild($envVarsNode)
        }
        
        # Helper function to add/update environment variable
        function Set-EnvVar {
            param([xml]$doc, [System.Xml.XmlElement]$parent, [string]$name, [string]$value)
            
            $existing = $parent.SelectSingleNode("environmentVariable[@name='$name']")
            if ($null -ne $existing) {
                $existing.SetAttribute("value", $value)
            } else {
                $newVar = $doc.CreateElement("environmentVariable")
                $newVar.SetAttribute("name", $name)
                $newVar.SetAttribute("value", $value)
                [void]$parent.AppendChild($newVar)
            }
        }
        
        Set-EnvVar $apiConfig $envVarsNode "AZURE_CLIENT_ID" $principalId
        Set-EnvVar $apiConfig $envVarsNode "KeyVaultName" $KeyVaultName
        Set-EnvVar $apiConfig $envVarsNode "ASPNETCORE_ENVIRONMENT" "Production"
        Set-EnvVar $apiConfig $envVarsNode "AzureOpenAI__Endpoint" $AzureOpenAIEndpoint
        Set-EnvVar $apiConfig $envVarsNode "AzureOpenAI__DeploymentName" $AzureOpenAIDeployment
        Set-EnvVar $apiConfig $envVarsNode "AzureTranslator__Endpoint" "https://api.cognitive.microsofttranslator.com"
        Set-EnvVar $apiConfig $envVarsNode "AzureTranslator__Region" "eastus"
        
        $backupPath = "$apiConfigPath.backup_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
        Copy-Item $apiConfigPath $backupPath -Force
        
        $apiConfig.Save($apiConfigPath)
        Write-Host "✅ API app web.config updated (backup: $backupPath)" -ForegroundColor Green
    } catch {
        Write-Host "❌ Error updating web.config: $_" -ForegroundColor Red
    }
} else {
    Write-Host "⚠️  web.config not found at: $apiConfigPath" -ForegroundColor Yellow
}

# Step 7: Recycle App Pools
Write-Host "[7/7] Recycling application pools..." -ForegroundColor Yellow
try {
    # Use Windows PowerShell for IIS management (PowerShell 7 has compatibility issues)
    $psPath = "$env:SystemRoot\System32\WindowsPowerShell\v1.0\powershell.exe"
    
    if (Test-Path $psPath) {
        # Recycle Web App Pool
        Write-Host "   Recycling $WebAppPoolName..." -ForegroundColor Gray
        $webResult = & $psPath -Command "Import-Module WebAdministration; if (Test-Path 'IIS:\AppPools\$WebAppPoolName') { Restart-WebAppPool -Name '$WebAppPoolName'; Write-Output 'SUCCESS' } else { Write-Output 'NOTFOUND' }"
        
        if ($webResult -eq 'SUCCESS') {
            Write-Host "✅ Recycled $WebAppPoolName" -ForegroundColor Green
        } elseif ($webResult -eq 'NOTFOUND') {
            Write-Host "⚠️  App pool '$WebAppPoolName' not found" -ForegroundColor Yellow
            
            # List available app pools
            Write-Host "   Available app pools:" -ForegroundColor Gray
            $pools = & $psPath -Command "Import-Module WebAdministration; Get-ChildItem IIS:\AppPools | Select-Object -ExpandProperty Name"
            $pools | ForEach-Object { Write-Host "     - $_" -ForegroundColor Gray }
        }
        
        # Recycle API App Pool
        Write-Host "   Recycling $ApiAppPoolName..." -ForegroundColor Gray
        $apiResult = & $psPath -Command "Import-Module WebAdministration; if (Test-Path 'IIS:\AppPools\$ApiAppPoolName') { Restart-WebAppPool -Name '$ApiAppPoolName'; Write-Output 'SUCCESS' } else { Write-Output 'NOTFOUND' }"
        
        if ($apiResult -eq 'SUCCESS') {
            Write-Host "✅ Recycled $ApiAppPoolName" -ForegroundColor Green
        } elseif ($apiResult -eq 'NOTFOUND') {
            Write-Host "⚠️  App pool '$ApiAppPoolName' not found" -ForegroundColor Yellow
        }
    } else {
        throw "Windows PowerShell not found"
    }
} catch {
    Write-Host "⚠️  Could not recycle app pools automatically. Please recycle manually:" -ForegroundColor Yellow
    Write-Host "   1. Open IIS Manager" -ForegroundColor Gray
    Write-Host "   2. Right-click '$WebAppPoolName' → Recycle" -ForegroundColor Gray
    Write-Host "   3. Right-click '$ApiAppPoolName' → Recycle" -ForegroundColor Gray
    Write-Host ""
    Write-Host "   Or run in Windows PowerShell:" -ForegroundColor Gray
    Write-Host "   Import-Module WebAdministration" -ForegroundColor Gray
    Write-Host "   Restart-WebAppPool -Name '$WebAppPoolName'" -ForegroundColor Gray
    Write-Host "   Restart-WebAppPool -Name '$ApiAppPoolName'" -ForegroundColor Gray
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Configuration Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "1. Verify HIMDS service is running:" -ForegroundColor White
Write-Host "   Get-Service himds" -ForegroundColor Gray
Write-Host ""
Write-Host "2. Test managed identity endpoint:" -ForegroundColor White
Write-Host "   `$url = 'http://localhost:40342/metadata/identity/oauth2/token?api-version=2020-06-01&resource=https://vault.azure.net'" -ForegroundColor Gray
Write-Host "   Invoke-RestMethod -Uri `$url -Headers @{Metadata='true'}" -ForegroundColor Gray
Write-Host ""
Write-Host "3. Check application logs:" -ForegroundColor White
Write-Host "   Get-Content '$WebAppPath\logs\stdout_*.log' -Tail 50" -ForegroundColor Gray
Write-Host ""
Write-Host "4. Test application:" -ForegroundColor White
Write-Host "   Browse to http://localhost:5011" -ForegroundColor Gray
Write-Host ""
Write-Host "Configuration Summary:" -ForegroundColor Yellow
Write-Host "  Machine Name: $MachineName" -ForegroundColor White
Write-Host "  Arc Resource Group: $ArcResourceGroup" -ForegroundColor White
Write-Host "  Principal ID: $principalId" -ForegroundColor White
Write-Host "  Key Vault: $KeyVaultName" -ForegroundColor White
Write-Host "  Web Path: $WebAppPath" -ForegroundColor White
Write-Host "  Web App Pool: $WebAppPoolName" -ForegroundColor White
Write-Host "  API Path: $ApiAppPath" -ForegroundColor White
Write-Host "  API App Pool: $ApiAppPoolName" -ForegroundColor White
Write-Host ""
Write-Host "✅ If both web.config files were updated successfully, your app should now authenticate via Arc managed identity!" -ForegroundColor Green
Write-Host "❌ If web.config updates failed, manually edit them using the guide: IIS-ENVIRONMENT-VARIABLES-GUIDE.md" -ForegroundColor Yellow
