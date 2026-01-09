using Projet_FullStack_FrontEnd.Components;
using Projet_FullStack_FrontEnd.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container for Blazor Server (.NET 8)
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
// Register HttpClient for components that expect it (e.g. Pokedex)
// Point HttpClient to the backend API base address. Adjust port if your API runs on a different port.
builder.Services.AddScoped(sp => new System.Net.Http.HttpClient
{
    BaseAddress = new System.Uri("http://localhost:5122/")
});

// Auth service to handle login/register and token management
builder.Services.AddScoped<Projet_FullStack_FrontEnd.Services.AuthService>();
// User state service to manage current user data across the app
builder.Services.AddScoped<Projet_FullStack_FrontEnd.Services.UserStateService>();

builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.MapHub<ChatHub>("/chathub");

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
    