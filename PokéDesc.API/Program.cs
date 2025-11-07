using MongoDB.Driver;
using PokéDesc.Data.Repositories;
using PokéDesc.Business.Services;

var builder = WebApplication.CreateBuilder(args);

// =================================================================
// 1. Configuration des Services
// =================================================================

// --- Configuration de la base de données MongoDB ---
// Lit la section "MongoDbSettings" depuis appsettings.json et le Secret Manager
var mongoDbSettings = builder.Configuration.GetSection("MongoDbSettings");
var connectionString = mongoDbSettings["ConnectionString"];
var databaseName = mongoDbSettings["DatabaseName"];

// Enregistre le client MongoDB pour qu'il soit réutilisable dans toute l'application
builder.Services.AddSingleton<IMongoClient>(serviceProvider =>
{
    return new MongoClient(connectionString);
});

// Enregistre l'instance de la base de données spécifique à notre projet
builder.Services.AddScoped<IMongoDatabase>(serviceProvider =>
{
    var client = serviceProvider.GetRequiredService<IMongoClient>();
    return client.GetDatabase(databaseName);
});

// --- Configuration de l'architecture N-tiers ---
// Enregistre le Repository (couche Data)
builder.Services.AddScoped<PokemonRepository>();

// Enregistre le Service (couche Business)
builder.Services.AddScoped<PokemonService>();

// --- Configuration des services de l'API ---
// **Ajoute les services nécessaires pour faire fonctionner les contrôleurs**
builder.Services.AddControllers(); 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// =================================================================
// 2. Construction de l'application
// =================================================================

var app = builder.Build();

// =================================================================
// 3. Configuration du Pipeline HTTP
// =================================================================

// Configure l'interface Swagger pour l'environnement de développement
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// **Indique à l'application d'utiliser les routes définies dans vos contrôleurs**
app.MapControllers();

app.Run();