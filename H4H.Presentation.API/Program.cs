using H4H.Application.Interfaces;
using H4H.Application.Services;
using H4H.Domain.Interfaces;
using H4H.Infrastructure.Data.Contexts;
using H4H.Infrastructure.Repositories;
using H4H.Infrastructure.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using H4H.Presentation.API;
using H4H.Presentation.API.Middleware;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Extensions.AspNetCore.Configuration.Secrets;

var builder = WebApplication.CreateBuilder(args);

// Load local development secrets (git-ignored file)
if (builder.Environment.IsDevelopment())
{
    var localSettingsPath = Path.Combine(builder.Environment.ContentRootPath, "appsettings.Development.Local.json");
    if (File.Exists(localSettingsPath))
    {
        builder.Configuration.AddJsonFile("appsettings.Development.Local.json", optional: true, reloadOnChange: true);
    }
}

// Configure Azure Key Vault (for production and local dev with Azure auth)
var keyVaultName = builder.Configuration["KeyVaultName"];
if (!string.IsNullOrEmpty(keyVaultName))
{
    var keyVaultUri = new Uri($"https://{keyVaultName}.vault.azure.net/");
    
    // Use DefaultAzureCredential for authentication (supports local dev + Azure)
    builder.Configuration.AddAzureKeyVault(
        keyVaultUri,
        new DefaultAzureCredential()
    );
}

// Add services to the container.

builder.Services.AddDbContext<H4HDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("H4HDB-DEV")));

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IVolunteerRepository, VolunteerRepository>();
builder.Services.AddScoped<IOrganizationRepository, OrganizationRepository>();
builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<IAddressRepository, AddressRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IChatSessionRepository, ChatSessionRepository>();
builder.Services.AddScoped<IChatMessageRepository, ChatMessageRepository>();



// Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IVolunteerService, VolunteerService>();
builder.Services.AddScoped<IOrganizationService, OrganizationService>();
builder.Services.AddScoped<IItemService, ItemService>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IChatService, ChatService>();

// AI Services
builder.Services.AddSingleton<IAzureTranslatorService, AzureTranslatorService>();
builder.Services.AddScoped<IChatOrchestrationService, ChatOrchestrationService>();


builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

// Configure CORS for Web frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebFrontend", policy =>
    {
        policy.WithOrigins(
            builder.Configuration["ApiSettings:AllowedOrigins"]?.Split(',') ?? new[]
            {
                "https://aidcircle.net",
                "https://www.aidcircle.net",
                "https://localhost:5011",  // Local dev - Web
                "http://localhost:5011"    // Local dev - Web (non-HTTPS)
            })
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Enable CORS
app.UseCors("AllowWebFrontend");

// Use dual authentication middleware (Azure AD B2C tokens + API key fallback)
app.UseDualAuthentication();

app.UseAuthorization();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }));

app.MapControllers();

app.Run();
