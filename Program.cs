using System.Net;
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
using SkinApi.Gui.Clients;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddMudServices();

var services = builder.Services;
services.AddControllers();

const string openIdConnectAuthenticationScheme = OpenIdConnectDefaults.AuthenticationScheme;

var openIdConfig = builder.Configuration.GetSection("Api");

services.AddAuthentication(options => { options.DefaultChallengeScheme = openIdConnectAuthenticationScheme; })
    .AddOpenIdConnect(openIdConnectAuthenticationScheme, options =>
    {
        var authority = $"{openIdConfig["Authority"]}";
        options.Authority = authority;

        var openIdConnectConfiguration = new OpenIdConnectConfiguration
        {
            TokenEndpoint = $"{openIdConfig["TokenEndpointPath"]}"
        };

        options.Configuration = openIdConnectConfiguration;

        options.ClientId = openIdConfig["ClientId"];
        options.ClientSecret = openIdConfig["ClientSecret"];
    });

services.AddClientAccessTokenManagement(options => { options.DefaultClient.Scope = openIdConfig["Scope"]; })
    .ConfigureBackchannelHttpClient(
        options => {
            options.BaseAddress = new Uri($"{openIdConfig["TokenEndpointPath"]}");
        }
    )
    .AddTransientHttpErrorPolicy(Policies.CreateRetryPolicy);

services.AddHttpClient<ISkinApiClient, SkinApiClient>(client =>
    {
        client.BaseAddress = new Uri(openIdConfig["Authority"]);
    })
    .AddClientAccessTokenHandler()
    .AddPolicyHandler(Policies.GetDefaultRetryPolicy())
    .AddPolicyHandler(Policies.GetDefaultCircuitBreakerPolicy());


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();


