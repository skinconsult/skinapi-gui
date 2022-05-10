using System.Net;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using MudBlazor.Services;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;
using SkinApi.Gui;
using SkinApi.Gui.Clients;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddMudServices();

var services = builder.Services;

// Configure backend communication to api
// This configures the Auth0 authentication 
var authConfig = builder.Configuration.GetSection("ApiAuth");
builder.Services.AddAccessTokenManagement(options =>
{
    options.Client.Clients.Add("skinapi", new ClientCredentialsTokenRequest
    {
        Address = authConfig["Address"],
        ClientId = authConfig["ClientId"],
        ClientSecret = authConfig["ClientSecret"],
        Scope = authConfig["Scope"],
        Parameters =
        {
            { "audience", authConfig["Audience"] },
        }
    });
});

// This links the 'skinapi' AccessTokenManaged client to the typed SkinApiClient
var apiConfig = builder.Configuration.GetSection("Api");
builder.Services
    .AddHttpClient<ISkinApiClient, SkinApiClient>()
    .ConfigureHttpClient(client => client.BaseAddress = new Uri(apiConfig["BaseAddress"]))
    .AddClientAccessTokenHandler("skinapi");


// Configuren front end authorization for login
var frontendAuthConfiguration = builder.Configuration.GetSection("FrontEndAuth");
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});


var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
if (!isDevelopment)
{
// Add authentication services
    services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        })
        .AddCookie()
        .AddOpenIdConnect("Auth0", options =>
        {
            // Set the authority to your Auth0 domain
            options.Authority = $"https://{frontendAuthConfiguration["Domain"]}";

            // Configure the Auth0 Client ID and Client Secret
            options.ClientId = frontendAuthConfiguration["ClientId"];
            options.ClientSecret = frontendAuthConfiguration["ClientSecret"];

            // Set response type to code
            options.ResponseType = OpenIdConnectResponseType.Code;

            // Configure the scope
            options.Scope.Add("openid");

            // Set the callback path, so Auth0 will call back to http://localhost:3000/callback
            // Also ensure that you have added the URL as an Allowed Callback URL in your Auth0 dashboard
            options.CallbackPath = new PathString("/callback");

            // Configure the Claims Issuer to be Auth0
            options.ClaimsIssuer = "Auth0";

            options.Events = new OpenIdConnectEvents
            {
                OnRedirectToIdentityProvider = context =>
                {
                    context.ProtocolMessage.SetParameter("audience", frontendAuthConfiguration["ApiIdentifier"]);

                    return Task.FromResult(0);
                },

                // handle the logout redirection
                OnRedirectToIdentityProviderForSignOut = context =>
                {
                    var logoutUri =
                        $"https://{frontendAuthConfiguration["Domain"]}/v2/logout?client_id={frontendAuthConfiguration["ClientId"]}";

                    var postLogoutUri = "/";
                    if (!string.IsNullOrEmpty(postLogoutUri))
                    {
                        if (postLogoutUri.StartsWith("/", StringComparison.InvariantCultureIgnoreCase))
                        {
                            // transform to absolute
                            var request = context.Request;
                            postLogoutUri = request.Scheme + "://" + request.Host + request.PathBase + postLogoutUri;
                        }

                        logoutUri += $"&returnTo={Uri.EscapeDataString(postLogoutUri)}";
                    }

                    context.Response.Redirect(logoutUri);
                    context.HandleResponse();

                    return Task.CompletedTask;
                },
            };
        });
}
else
{
    services.AddAuthentication("BasicAuthentication")
        .AddScheme<AuthenticationSchemeOptions, 
            MockAuthenticatedUser>("BasicAuthentication", null);
}


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();