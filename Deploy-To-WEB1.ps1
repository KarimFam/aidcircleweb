# Deploy to WEB1 with Web.Config Preservation
# Run this ON WEB1 server or with access to E:\aidcircle

Write-Host "=== Deploying Updated Applications to WEB1 ===" -ForegroundColor Cyan
Write-Host ""

$publishWebPath = "O:\source\repos\aidcircleweb\publish\web"
$publishApiPath = "O:\source\repos\aidcircleweb\publish\api"
$targetWebPath = "E:\aidcircle\web"
$targetApiPath = "E:\aidcircle\api"

# Step 1: Backup existing web.config files
Write-Host "[1/6] Backing up existing web.config files..." -ForegroundColor Yellow
Copy-Item "$targetWebPath\web.config" "$targetWebPath\web.config.backup-$(Get-Date -Format 'yyyyMMdd-HHmmss')" -ErrorAction SilentlyContinue
Copy-Item "$targetApiPath\web.config" "$targetApiPath\web.config.backup-$(Get-Date -Format 'yyyyMMdd-HHmmss')" -ErrorAction SilentlyContinue
Write-Host "✅ Backups created" -ForegroundColor Green
Write-Host ""

# Step 2: Stop app pools
Write-Host "[2/6] Stopping IIS App Pools..." -ForegroundColor Yellow
$psPath = "$env:SystemRoot\System32\WindowsPowerShell\v1.0\powershell.exe"
& $psPath -Command "Import-Module WebAdministration; Stop-WebAppPool -Name 'aidcircle-web'"
& $psPath -Command "Import-Module WebAdministration; Stop-WebAppPool -Name 'aidcircle-api'"
Write-Host "✅ App pools stopped" -ForegroundColor Green
Start-Sleep -Seconds 5
Write-Host ""

# Step 3: Copy Web app files (exclude web.config)
Write-Host "[3/6] Deploying Web application..." -ForegroundColor Yellow
Get-ChildItem -Path $publishWebPath -Exclude "web.config" | Copy-Item -Destination $targetWebPath -Recurse -Force
Write-Host "✅ Web app deployed" -ForegroundColor Green
Write-Host ""

# Step 4: Copy API files (exclude web.config)
Write-Host "[4/6] Deploying API application..." -ForegroundColor Yellow
Get-ChildItem -Path $publishApiPath -Exclude "web.config" | Copy-Item -Destination $targetApiPath -Recurse -Force
Write-Host "✅ API deployed" -ForegroundColor Green
Write-Host ""

# Step 5: Ensure web.config has AZURE_CLIENT_ID
Write-Host "[5/6] Verifying web.config environment variables..." -ForegroundColor Yellow

function Ensure-AzureClientId {
    param($configPath)
    
    [xml]$xml = Get-Content $configPath
    $envVars = $xml.configuration.location.system.webServer.aspNetCore.environmentVariables.environmentVariable
    
    $clientIdVar = $envVars | Where-Object { $_.name -eq "AZURE_CLIENT_ID" }
    
    if (-not $clientIdVar) {
        Write-Host "⚠️  AZURE_CLIENT_ID missing in $configPath, adding it..." -ForegroundColor Yellow
        
        $newVar = $xml.CreateElement("environmentVariable")
        $newVar.SetAttribute("name", "AZURE_CLIENT_ID")
        $newVar.SetAttribute("value", "cc261686-88bf-4252-84c1-44b28dd1c533")
        
        $xml.configuration.location.system.webServer.aspNetCore.environmentVariables.AppendChild($newVar) | Out-Null
        $xml.Save($configPath)
        
        Write-Host "✅ AZURE_CLIENT_ID added to $configPath" -ForegroundColor Green
    } else {
        Write-Host "✅ AZURE_CLIENT_ID already set: $($clientIdVar.value)" -ForegroundColor Green
    }
}

Ensure-AzureClientId -configPath "$targetWebPath\web.config"
Ensure-AzureClientId -configPath "$targetApiPath\web.config"
Write-Host ""

# Step 6: Start app pools
Write-Host "[6/6] Starting IIS App Pools..." -ForegroundColor Yellow
& $psPath -Command "Import-Module WebAdministration; Start-WebAppPool -Name 'aidcircle-web'"
& $psPath -Command "Import-Module WebAdministration; Start-WebAppPool -Name 'aidcircle-api'"
Write-Host "✅ App pools started" -ForegroundColor Green
Write-Host ""

Write-Host "Waiting 15 seconds for applications to start..." -ForegroundColor Gray
Start-Sleep -Seconds 15

Write-Host ""
Write-Host "=== Deployment Complete ===" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Check web app logs: Get-Content 'E:\aidcircle\web\logs\stdout_*.log' -Tail 30" -ForegroundColor White
Write-Host "   Look for: [KeyVault] AZURE_CLIENT_ID from environment: cc261686-88bf-4252-84c1-44b28dd1c533" -ForegroundColor Gray
Write-Host ""
Write-Host "2. Check API logs: Get-Content 'E:\aidcircle\api\logs\stdout_*.log' -Tail 30" -ForegroundColor White
Write-Host ""
Write-Host "3. Test web app: Invoke-WebRequest -Uri 'http://localhost:5011' -UseBasicParsing" -ForegroundColor White
Write-Host ""
