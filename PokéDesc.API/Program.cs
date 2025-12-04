using MongoDB.Driver;
using PokéDesc.Data.Repositories;
using PokéDesc.Business.Services;
using PokéDesc.Business.Interfaces;
//using PokéDesc.Business.Services;

// --- AJOUT ---
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
// --- FIN AJOUT ---

var builder = WebApplication.CreateBuilder(args);

// =================================================================
// 1. Configuration des Services
// =================================================================

// --- AJOUT : Configuration de l'Authentification JWT ---
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        // On récupère la clé secrète (via le Secret Manager ou appsettings)
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("La clé JWT 'Jwt:Key' est manquante dans la configuration.")))
    };
});
// --- FIN AJOUT ---


// --- Configuration de la base de données MongoDB ---
// (Ton code existant reste ici)
var mongoDbSettings = builder.Configuration.GetSection("MongoDbSettings");
var connectionString = mongoDbSettings["ConnectionString"];
var databaseName = mongoDbSettings["DatabaseName"];

builder.Services.AddSingleton<IMongoClient>(serviceProvider =>
{
    return new MongoClient(connectionString);
});

builder.Services.AddScoped<IMongoDatabase>(serviceProvider =>
{
    var client = serviceProvider.GetRequiredService<IMongoClient>();
    return client.GetDatabase(databaseName);
});

// --- Configuration de l'architecture N-tiers ---
// Enregistre le Repository (couche Data)
builder.Services.AddScoped<PokemonRepository>();

// Enregistre le Service (couche Business)
builder.Services.AddScoped<IPokemonService, PokemonService>();
builder.Services.AddScoped<IPartieService, PartieService>();

// --- Configuration des services de l'API ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // 1. Définir le schéma de sécurité (comment on s'authentifie)
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Entrez 'Bearer' [espace] puis votre token. \n\nExemple : 'Bearer eyJhbGciOi...'"
    });

    // 2. Dire à Swagger d'appliquer ce schéma à tous les endpoints
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Enregistre les dépôts et services personnalisés
builder.Services.AddScoped<DresseurRepository>();
builder.Services.AddScoped<DresseurService>();

// --- CORS pour autoriser le frontend local (développement) ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocal", policy =>
    {
        policy.WithOrigins("http://localhost:5058", "https://localhost:7217")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// =================================================================
// 2. Construction de l'application
// =================================================================

var app = builder.Build();

// =================================================================
// 3. Configuration du Pipeline HTTP
// =================================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Autoriser CORS avant l'authentification/autorisation
app.UseCors("AllowLocal");

// --- AJOUT : Activation de l'authentification ---
// Doit être AVANT MapControllers
app.UseAuthentication(); // Qui es-tu ? (lit le token)
app.UseAuthorization();  // As-tu le droit ? (vérifie [Authorize])
// --- FIN AJOUT ---


// **Indique à l'application d'utiliser les routes définies dans vos contrôleurs**
app.MapControllers();

app.Run();