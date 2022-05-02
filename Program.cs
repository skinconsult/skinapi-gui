using System.Net;
using IdentityModel.Client;
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
            { "audience", authConfig["Audience"]},
        }
    });
});

var apiConfig = builder.Configuration.GetSection("Api");
builder.Services
    .AddHttpClient<ISkinApiClient, SkinApiClient>()
    .ConfigureHttpClient(client => client.BaseAddress = new Uri(apiConfig["BaseAddress"]))
    .AddClientAccessTokenHandler("skinapi");


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


