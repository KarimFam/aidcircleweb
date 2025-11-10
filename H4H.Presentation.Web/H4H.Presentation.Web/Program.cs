using H4H.Infrastructure.Repositories;
using H4H.Infrastructure.Services;
using H4H.Domain.Interfaces;
using H4H.Domain.Entities;
using H4H.Infrastructure.Data.Contexts;
using H4H.Presentation.Web.Client.Pages;
using H4H.Presentation.Web.Components;
using H4H.Presentation.Web.Services;
using H4H.Presentation.Web.Components.Services;
using Microsoft.EntityFrameworkCore;
using H4H.Application.Services;
using H4H.Application.Interfaces;
using Microsoft.Identity.Web;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.Security.Claims;
using System.Reflection;
using System.Linq.Dynamic.Core;
using Microsoft.Identity.Web.UI;
using Microsoft.AspNetCore.Authorization;
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

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddMicrosoftIdentityConsentHandler()
    .AddInteractiveWebAssemblyComponents();

//REST Client
builder.Services.AddHttpClient();

//Identity and Authentication
builder.Services.AddControllersWithViews()
    .AddMicrosoftIdentityUI();
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<HttpContextAccessor>();

// Remove global authentication requirement - allow public pages
// Individual pages use [Authorize] attribute as needed
builder.Services.AddAuthorization(config =>
{
    config.AddPolicy("Volunteer", policy => policy.RequireClaim("IsVolunteer", "true"));
});

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(options =>
                {
                    
                    builder.Configuration.Bind("AzureAd", options);
                    options.Events = new OpenIdConnectEvents
                    {
                        OnRedirectToIdentityProvider = async ctxt =>
                        {
                            // Invoked before redirecting to the identity provider to authenticate. 
                            // This can be used to set ProtocolMessage.State
                            // that will be persisted through the authentication process. 
                            // The ProtocolMessage can also be used to add or customize
                            // parameters sent to the identity provider.
                            await Task.Yield();
                        },
                        OnAuthenticationFailed = async ctxt =>
                        {
                            // They tried to log in but it failed
                            await Task.Yield();
                        },
                        OnSignedOutCallbackRedirect = async ctxt =>
                        {
                            ctxt.HttpContext.Response.Redirect(ctxt.Options.SignedOutRedirectUri);
                            ctxt.HandleResponse();
                            await Task.Yield();
                        },
                        OnTicketReceived = async ctxt =>
                        {
                            if (ctxt.Principal != null)
                            {
                                if (ctxt.Principal.Identity is ClaimsIdentity identity)
                                {
                                    var colClaims = await ctxt.Principal.Claims.ToDynamicListAsync();
                                    var IdentityProvider = colClaims.FirstOrDefault(
                                        c => c.Type == "http://schemas.microsoft.com/identity/claims/identityprovider")?.Value;
                                    var Objectidentifier = colClaims.FirstOrDefault(
                                        c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
                                    var EmailAddress = colClaims.FirstOrDefault(
                                        c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;
                                    var FirstName = colClaims.FirstOrDefault(
                                        c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname")?.Value;
                                    var LastName = colClaims.FirstOrDefault(
                                        c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname")?.Value;
                                    var AzureB2CFlow = colClaims.FirstOrDefault(
                                        c => c.Type == "http://schemas.microsoft.com/claims/authnclassreference")?.Value;
                                    var auth_time = colClaims.FirstOrDefault(
                                        c => c.Type == "auth_time")?.Value;
                                    var DisplayName = colClaims.FirstOrDefault(
                                        c => c.Type == "name")?.Value;
                                    var idp_access_token = colClaims.FirstOrDefault(
                                        c => c.Type == "idp_access_token")?.Value;

                                    // Auto-create user in database if doesn't exist
                                    if (!string.IsNullOrEmpty(Objectidentifier))
                                    {
                                        var userService = ctxt.HttpContext.RequestServices.GetRequiredService<IUserService>();
                                        var existingUser = await userService.GetByExternalAuthIdAsync(Objectidentifier);
                                        
                                        if (existingUser == null)
                                        {
                                            // Create new user
                                            var newUser = new User
                                            {
                                                UserId = Guid.NewGuid(),
                                                ExternalAuthId = Objectidentifier,
                                                ExternalAuthProvider = "AzureAD", // Simplified instead of full URL
                                                Email = EmailAddress ?? $"{Objectidentifier}@placeholder.com",
                                                FirstName = FirstName ?? "User",
                                                LastName = LastName ?? "",
                                                Username = DisplayName ?? EmailAddress ?? Objectidentifier,
                                                IsActive = true,
                                                DateOfBirth = DateTime.Now.AddYears(-25), // Default placeholder
                                                CreatedDate = DateTime.Now,
                                                ModifiedDate = DateTime.Now,
                                                Addresses = new List<Address>(),
                                                Items = new List<Item>(),
                                                Orders = new List<Order>()
                                            };
                                            
                                            await userService.AddUserAsync(newUser);
                                        }
                                    }
                                }
                            }
                            await Task.Yield();
                        },
                    };
                });



// Database Connection - Use DbContextPool for improved performance and scalability in high-concurrency scenarios (e.g., Blazor Server, API endpoints)
builder.Services.AddDbContextPool<H4HDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("H4HDB-DEV"),
        sqlServerOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        }));

// Services
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<IItemService, ItemService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrganizationService, OrganizationService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IVolunteerService, VolunteerService>();
builder.Services.AddScoped<IChatService, ChatService>();

// Hybrid service for API/Direct access
builder.Services.AddScoped<IHybridChatService, HybridChatService>();

// AI Services
builder.Services.AddSingleton<IAzureTranslatorService, AzureTranslatorService>();
builder.Services.AddScoped<IChatOrchestrationService, ChatOrchestrationService>();

// API Client for backend communication
builder.Services.AddHttpClient<IApiClient, ApiClient>(client =>
{
    var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:5135";
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Hybrid services (API client + direct fallback)
builder.Services.AddScoped<IHybridChatService, HybridChatService>();

// Repositories (keep for direct database access if needed, but prefer API client)
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrganizationRepository, OrganizationRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IVolunteerRepository, VolunteerRepository>();
builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<IAddressRepository, AddressRepository>();
builder.Services.AddScoped<IChatSessionRepository, ChatSessionRepository>();
builder.Services.AddScoped<IChatMessageRepository, ChatMessageRepository>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Enable HTTPS redirection except in development
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();
app.UseAntiforgery();

// Authentication MUST come before authorization and MapRazorComponents
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(H4H.Presentation.Web.Client._Imports).Assembly);

app.MapControllers();

app.Run();
