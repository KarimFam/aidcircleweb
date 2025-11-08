# Quick Command Snippets for AidCircle Development

## Database Migrations

### Create New Migration
```powershell
dotnet ef migrations add YourMigrationName `
  --project H4H.Infrastructure `
  --startup-project H4H.Presentation.API
```

### Apply Migrations
```powershell
dotnet ef database update `
  --project H4H.Infrastructure `
  --startup-project H4H.Presentation.API
```

### Rollback Migration
```powershell
dotnet ef database update PreviousMigrationName `
  --project H4H.Infrastructure `
  --startup-project H4H.Presentation.API
```

### Remove Last Migration (if not applied)
```powershell
dotnet ef migrations remove `
  --project H4H.Infrastructure `
  --startup-project H4H.Presentation.API
```

### Generate SQL Script
```powershell
dotnet ef migrations script `
  --project H4H.Infrastructure `
  --startup-project H4H.Presentation.API `
  --output migration.sql
```

## Build & Run

### Clean Build
```powershell
dotnet clean H4H.sln --configuration Release
dotnet restore H4H.sln
dotnet build H4H.sln --configuration Release
```

### Run API
```powershell
dotnet run --project H4H.Presentation.API
```

### Run Web App
```powershell
dotnet run --project H4H.Presentation.Web/H4H.Presentation.Web
```

### Watch Mode (auto-reload)
```powershell
dotnet watch --project H4H.Presentation.Web/H4H.Presentation.Web
```

## Publish

### Publish API
```powershell
dotnet publish H4H.Presentation.API/H4H.Presentation.API.csproj `
  --configuration Release `
  --output ./publish/api
```

### Publish Web
```powershell
dotnet publish H4H.Presentation.Web/H4H.Presentation.Web/H4H.Presentation.Web.csproj `
  --configuration Release `
  --output ./publish/web
```

## Azure Deployment

### Deploy to Azure App Service
```powershell
az webapp deploy `
  --name "app-aidcircle-web-prod" `
  --resource-group "rg-aidcircle-prod" `
  --src-path "./publish/web" `
  --type zip
```

## Troubleshooting

### Kill All dotnet Processes
```powershell
taskkill /F /IM dotnet.exe
```

### Check Running Processes
```powershell
Get-Process | Where-Object {$_.ProcessName -eq "dotnet"}
```

### View App Logs
```powershell
az webapp log tail `
  --name "app-aidcircle-web-prod" `
  --resource-group "rg-aidcircle-prod"
```

## Testing

### Run All Tests
```powershell
dotnet test H4H.sln --configuration Release --verbosity normal
```

### Run Specific Test Project
```powershell
dotnet test H4H.Tests/H4H.Tests.csproj
```

## Package Management

### Add NuGet Package
```powershell
dotnet add H4H.Infrastructure/H4H.Infrastructure.csproj package PackageName
```

### Restore Packages
```powershell
dotnet restore H4H.sln
```

### List Outdated Packages
```powershell
dotnet list package --outdated
```
