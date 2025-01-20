using H4H.Infrastructure.Repositories;
using H4H.Domain.Interfaces;
using H4H.Infrastructure.Data.Contexts;
using H4H.Presentation.Web.Client.Pages;
using H4H.Presentation.Web.Components;
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




var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddAuthorization(config =>
{
    config.AddPolicy("Volunteer", policy => policy.RequireClaim("IsVolunteer", "true"));
});

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(options =>
                {
                    
                    builder.Configuration.Bind("AzureAdB2C", options);
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
                                }
                            }
                            await Task.Yield();
                        },
                    };
                });



//Database Connection
builder.Services.AddDbContext<H4HDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("H4HDB-DEV")));

// Services
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<IItemService, ItemService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrganizationService, OrganizationService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IVolunteerService, VolunteerService>();
builder.Services.AddScoped<IWeatherService, WeatherService>();

// Repositories
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrganizationRepository, OrganizationRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IVolunteerRepository, VolunteerRepository>();
builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<IAddressRepository, AddressRepository>();
builder.Services.AddScoped<IWeatherRepository, WeatherRepository>();

// Add HttpClient for WeatherService
builder.Services.AddHttpClient<IWeatherRepository, WeatherRepository>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();

    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//app.UseAuthentication();
app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(H4H.Presentation.Web.Client._Imports).Assembly);

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
