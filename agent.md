# Guide de Développement - PokéDesc (PokeGuessr)

## 📋 Vue d'ensemble

PokéDesc est une application full-stack .NET 8 de type jeu de devinettes Pokémon. L'architecture suit une séparation claire en couches avec une API REST backend et un frontend Blazor Server.

---

## 🏗️ Architecture Globale

### Structure des Projets

```
PokéDesc.API/           → Couche Présentation (API REST)
PokéDesc.Business/      → Couche Métier (Logique applicative)
PokéDesc.Data/          → Couche Accès aux Données (Repositories)
PokéDesc.Domain/        → Couche Domaine (Entités et Modèles)
Projet_FullStack_FrontEnd/ → Frontend Blazor Server
```

### Flux de Dépendances

```
Frontend → API → Business → Data → Domain
                              ↓
                          MongoDB
```

**Règle stricte** : Une couche ne peut dépendre que des couches inférieures. Jamais l'inverse.

---

## 📦 Description des Couches

### 1. **PokéDesc.Domain** (Couche Domaine)

**Responsabilité** : Définir les entités métier et les modèles de données purs.

**Contenu** :
- **Entités principales** : `Pokemon`, `Partie`, `Dresseur`, `PokemonCapture`
- **Modèles de données** : `Stats`, `Physical`, `Ability`, `Generation`, `Region`, `Status`, etc.
- **Aucune logique métier**, uniquement des propriétés et attributs MongoDB

**Technologies** :
- MongoDB.Bson (pour les attributs `[BsonId]`, `[BsonElement]`, etc.)

**Conventions** :
- Classes en PascalCase
- Propriétés publiques avec get/set
- Utiliser `[BsonId]` et `[BsonRepresentation(BsonType.ObjectId)]` pour les IDs MongoDB
- Utiliser `[BsonElement("nom_champ")]` pour mapper les noms de champs MongoDB
- Nullable désactivé (`<Nullable>disable</Nullable>`) dans le .csproj

**Exemple** :
```csharp
public class Pokemon
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("name_fr")]
    public string NameFr { get; set; }

    [BsonElement("pokedex_number")]
    public int PokedexNumber { get; set; }
    
    // ... autres propriétés
}
```

---

### 2. **PokéDesc.Data** (Couche Accès aux Données)

**Responsabilité** : Gérer l'accès à la base de données MongoDB via le pattern Repository.

**Contenu** :
- **Repositories** : Classes qui encapsulent les opérations CRUD sur MongoDB
- Exemples : `PokemonRepository`, `DresseurRepository`

**Technologies** :
- MongoDB.Driver

**Conventions** :
- Un Repository par entité principale
- Nommage : `{Entity}Repository`
- Injecter `IMongoDatabase` dans le constructeur
- Méthodes asynchrones avec suffixe `Async`
- Retourner des entités du Domain ou `null`

**Structure type d'un Repository** :
```csharp
public class PokemonRepository
{
    private readonly IMongoCollection<Pokemon> _collection;

    public PokemonRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<Pokemon>("Pokemon_Collection");
    }

    public async Task<List<Pokemon>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task<Pokemon?> GetByIdAsync(string id)
    {
        return await _collection.Find(p => p.Id == id).FirstOrDefaultAsync();
    }

    // Autres méthodes CRUD...
}
```

**Dépendances** : `PokéDesc.Domain` uniquement

---

### 3. **PokéDesc.Business** (Couche Métier)

**Responsabilité** : Contenir toute la logique métier de l'application.

**Contenu** :
- **Services** : Implémentent la logique métier complexe
  - `PokemonService`, `PartieService`, `DresseurService`
- **Interfaces** : Définissent les contrats des services
  - `IPokemonService`, `IPartieService`
- **Models** : DTOs métier spécifiques à la logique applicative
  - `GuessResult`, `PokemonHints`

**Technologies** :
- BCrypt.Net-Next (hachage de mots de passe)
- System.IdentityModel.Tokens.Jwt (génération JWT)

**Conventions** :
- Un Service par agrégat métier principal
- Nommage : `{Entity}Service` et `I{Entity}Service`
- Injecter les Repositories et autres services nécessaires
- Toutes les méthodes publiques doivent être asynchrones (`async Task`)
- Lever des exceptions typées pour les erreurs métier :
  - `KeyNotFoundException` : Entité introuvable
  - `ArgumentException` : Paramètre invalide
  - `InvalidOperationException` : Opération invalide dans l'état actuel
- Les services doivent implémenter leur interface

**Structure type d'un Service** :
```csharp
public interface IPokemonService
{
    Task<List<Pokemon>> GetAllPokemonsAsync();
    Task<Pokemon> GetPokemonByIdAsync(string id);
    // ... autres méthodes
}

public class PokemonService : IPokemonService
{
    private readonly PokemonRepository _repository;

    public PokemonService(PokemonRepository repository)
    {
        _repository = repository;
    }

    public async Task<Pokemon> GetPokemonByIdAsync(string id)
    {
        var pokemon = await _repository.GetByIdAsync(id);
        if (pokemon == null)
        {
            throw new KeyNotFoundException($"Pokemon avec l'ID {id} introuvable");
        }
        return pokemon;
    }

    // ... logique métier complexe
}
```

**Dépendances** : `PokéDesc.Domain`, `PokéDesc.Data`

---

### 4. **PokéDesc.API** (Couche Présentation - API REST)

**Responsabilité** : Exposer les fonctionnalités via une API REST HTTP.

**Contenu** :
- **Controllers** : Points d'entrée HTTP (endpoints)
  - `PokemonController`, `PartieController`, `DresseursController`
- **DTOs** : Objets de transfert pour les requêtes/réponses
  - `CreateGameRequest`, `JoinGameRequest`, `SubmitGuessRequest`, etc.
- **Program.cs** : Configuration de l'application (DI, middleware, authentification)

**Technologies** :
- ASP.NET Core Web API
- Swagger/OpenAPI
- JWT Bearer Authentication
- MongoDB.Driver (configuration)

**Conventions** :
- Controllers héritent de `ControllerBase`
- Attributs `[ApiController]` et `[Route("api/[controller]")]`
- Nommage : `{Entity}Controller`
- Injecter uniquement les interfaces de services (jamais les Repositories directement)
- Méthodes HTTP avec attributs : `[HttpGet]`, `[HttpPost]`, `[HttpPut]`, `[HttpDelete]`
- Retourner `IActionResult` ou `ActionResult<T>`
- Gérer les exceptions et retourner les codes HTTP appropriés :
  - 200 OK : Succès
  - 201 Created : Création réussie
  - 400 Bad Request : Validation échouée
  - 401 Unauthorized : Non authentifié
  - 404 Not Found : Ressource introuvable
  - 500 Internal Server Error : Erreur serveur

**Structure type d'un Controller** :
```csharp
[ApiController]
[Route("api/[controller]")]
public class PartieController : ControllerBase
{
    private readonly IPartieService _partieService;

    public PartieController(IPartieService partieService)
    {
        _partieService = partieService;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateGame([FromBody] CreateGameRequest request)
    {
        var partie = await _partieService.CreateGameAsync(request.DresseurId);
        return Ok(partie);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetGame(string id)
    {
        try
        {
            var partie = await _partieService.GetGameAsync(id);
            return Ok(partie);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}
```

**DTOs** :
- Classes simples avec propriétés publiques
- Utiliser `[Required]` pour la validation
- Initialiser les strings à `string.Empty` pour éviter les nulls

**Configuration DI dans Program.cs** :
```csharp
// MongoDB
builder.Services.AddSingleton<IMongoClient>(sp => new MongoClient(connectionString));
builder.Services.AddScoped<IMongoDatabase>(sp => client.GetDatabase(databaseName));

// Repositories
builder.Services.AddScoped<PokemonRepository>();

// Services
builder.Services.AddScoped<IPokemonService, PokemonService>();
builder.Services.AddScoped<IPartieService, PartieService>();
```

**Dépendances** : `PokéDesc.Business` uniquement (pas d'accès direct à Data ou Domain)

---

### 5. **Projet_FullStack_FrontEnd** (Frontend Blazor Server)

**Responsabilité** : Interface utilisateur interactive.

**Contenu** :
- **Components/Pages/** : Pages Razor (`.razor`)
  - `Home.razor`, `Partie.razor`, `Pokedex.razor`, `Login.razor`, etc.
- **Components/Layout/** : Layouts partagés
- **Services/** : Services frontend
  - `AuthService` : Gestion authentification JWT et localStorage
  - `UserStateService` : Gestion état utilisateur
- **wwwroot/** : Assets statiques (CSS, images, JS)

**Technologies** :
- Blazor Server (.NET 8)
- HttpClient (appels API)
- JSInterop (localStorage, JS interop)

**Conventions** :
- Composants Razor : PascalCase
- Utiliser `@inject` pour l'injection de dépendances
- Séparer la logique dans des blocs `@code { }`
- CSS scoped : `{Component}.razor.css`
- Appels API via HttpClient injecté (configuré avec `BaseAddress`)
- Gérer les états de chargement (`isLoading`) et erreurs (`errorMessage`)
- Utiliser `RequireAuth` pour les pages nécessitant authentification

**Direction artistique (Pokédex Gen 1, moderne/minimaliste, public jeune)** :
- Palette : Rouge primaire `#E53935`, Bleu secondaire `#1E88E5`, Fond clair `#F7F9FC`, Blanc pur `#FFFFFF`, Gris neutres `#D9E1EC` (bordures) et `#546E7A`/`#0F172A` (texte). États : Succès `#2E7D32`, Alerte `#ffa500`, Danger `#D32F2F`.
- Typo : Police moderne sans-serif (Inter ou Poppins, fallback Segoe UI). Hiérarchie : H1 28-32px, H2 22-24px, H3 18-20px, corps 15-16px, petit 13px. Poids : titres 700/600, corps 400-500.
- Layout : Largeur max 1200px, padding horizontal 16-24px. Grille responsive minmax(260px, 1fr) gap 16-20px. Spacing vertical 24-32px (sections), 8-12px (éléments). Header sticky ~64px, fond blanc, ombre légère.
- Composants communs :
    - Boutons : primaire rouge (texte blanc), rayon 10-12px, h≈44px, hover ombre douce + légère translation. Secondaire : contour bleu sur fond blanc.
    - Cartes : fond blanc, rayon 12-14px, bordure 1px `#D9E1EC`, ombre très légère, padding 16-20px.
    - Inputs : h≈44px, bordure 1px `#D9E1EC`, rayon 10px, focus bordure + glow bleu.
    - Badges : fond bleu clair (10% opacité), texte bleu, rayon pill. États en déclinaisons douces.
    - Modales : fond blanc, rayon 14px, overlay sombre 40%, bouton principal rouge.
- Background : page `#F7F9FC`, panneaux blancs ponctuels. Pas d'illustrations ou d’éléments visuels lourds pour l’instant ; icônes simples monochromes bleu/gris si besoin.
- Navigation : barre top sticky, états actifs en bleu, hover rouge/bleu léger. Breadcrumbs simples si nécessaire.
- États/feedback : toasts/alertes discrètes (succès vert, alerte orange, erreur rouge, info bleu). Loader : spinner bleu. Focus visible anneau bleu 2px.
- Responsive : Mobile colonne unique, paddings 12-16px ; tablette/desktop 2-3 colonnes. Cibles tactiles min 44px.
- Pages (home/login déjà faites) : aligner Partie, Pokédex, PokemonDetails, Profil/Amis, Succes/Objets/Mini-jeux sur les mêmes patterns (cartes + boutons + badges + inputs). Pokedex en grille, filtres en pills ; Partie avec cartes statut joueurs et code session en badge ; détails Pokémon avec carte principale + onglets.
- Tokens CSS recommandés :
    ```css
    :root {
        --color-red: #E53935;
        --color-blue: #1E88E5;
        --color-bg: #F7F9FC;
        --color-white: #FFFFFF;
        --color-border: #D9E1EC;
        --color-text: #0F172A;
        --color-text-muted: #546E7A;
        --shadow-sm: 0 4px 12px rgba(15, 23, 42, 0.06);
        --radius-md: 12px;
        --space-xs: 8px;
        --space-sm: 12px;
        --space-md: 16px;
        --space-lg: 24px;
    }
    ```

**Structure type d'une Page Razor** :
```csharp
@page "/partie"
@inject HttpClient Http
@inject AuthService AuthService
@inject NavigationManager NavigationManager

<RequireAuth>
    @if (isLoading)
    {
        <p><em>Chargement…</em></p>
    }
    else if (!string.IsNullOrEmpty(errorMessage))
    {
        <p class="text-danger">@errorMessage</p>
    }
    else
    {
        <!-- Contenu principal -->
    }
</RequireAuth>

@code {
    private bool isLoading = true;
    private string errorMessage = "";
    private MyData? data;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            data = await Http.GetFromJsonAsync<MyData>("api/endpoint");
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
        }
        finally
        {
            isLoading = false;
        }
    }
}
```

**Services Frontend** :
- `AuthService` : Gérer JWT, login/logout, extraction claims
- `UserStateService` : État global de l'utilisateur connecté
- Enregistrer les services dans `Program.cs` avec `AddScoped`

**Dépendances** : Aucune référence aux projets backend (communication via HTTP uniquement)

---

## 🎯 Principes de Code et Best Practices

### Principes SOLID

1. **Single Responsibility Principle (SRP)**
   - Chaque classe a une seule raison de changer
   - Les Controllers ne font que du routage HTTP
   - Les Services contiennent la logique métier
   - Les Repositories ne font que l'accès aux données

2. **Open/Closed Principle (OCP)**
   - Utiliser des interfaces pour l'extensibilité
   - Ajouter des fonctionnalités via de nouvelles implémentations, pas en modifiant l'existant

3. **Liskov Substitution Principle (LSP)**
   - Les implémentations d'interfaces doivent être interchangeables
   - Respecter les contrats définis par les interfaces

4. **Interface Segregation Principle (ISP)**
   - Interfaces ciblées et spécifiques
   - Ne pas forcer les implémentations à dépendre de méthodes inutilisées

5. **Dependency Inversion Principle (DIP)**
   - Dépendre des abstractions (interfaces), pas des implémentations concrètes
   - Utiliser l'injection de dépendances systématiquement

### Conventions de Nommage

| Élément | Convention | Exemple |
|---------|-----------|---------|
| Classes | PascalCase | `PokemonService` |
| Interfaces | I + PascalCase | `IPokemonService` |
| Méthodes | PascalCase | `GetPokemonByIdAsync` |
| Paramètres | camelCase | `pokemonId`, `dresseurName` |
| Variables locales | camelCase | `isLoading`, `errorMessage` |
| Propriétés publiques | PascalCase | `PokedexNumber`, `NameFr` |
| Champs privés | _camelCase | `_repository`, `_service` |
| Constantes | PascalCase | `MaxAttempts`, `BaseScore` |
| Méthodes async | Suffixe Async | `CreateGameAsync` |

### Gestion des Erreurs

**Backend (API/Business)** :
- Lever des exceptions typées dans la couche Business
- Attraper et transformer en réponses HTTP dans les Controllers
- Ne jamais exposer les stack traces en production

```csharp
// Business Layer
public async Task<Pokemon> GetPokemonByIdAsync(string id)
{
    var pokemon = await _repository.GetByIdAsync(id);
    if (pokemon == null)
    {
        throw new KeyNotFoundException($"Pokemon avec l'ID {id} introuvable");
    }
    return pokemon;
}

// API Layer
[HttpGet("{id}")]
public async Task<IActionResult> GetById(string id)
{
    try
    {
        var pokemon = await _service.GetPokemonByIdAsync(id);
        return Ok(pokemon);
    }
    catch (KeyNotFoundException ex)
    {
        return NotFound(new { message = ex.Message });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { message = "Erreur serveur", error = ex.Message });
    }
}
```

**Frontend** :
- Utiliser des blocs `try-catch` pour les appels HTTP
- Afficher des messages d'erreur conviviaux
- Logger les erreurs avec `ILogger` si nécessaire

```csharp
try
{
    data = await Http.GetFromJsonAsync<MyData>("api/endpoint");
}
catch (Exception ex)
{
    errorMessage = "Impossible de charger les données. Veuillez réessayer.";
    Logger.LogError(ex, "Erreur lors du chargement des données");
}
```

### Asynchronisme

- **Toujours** utiliser `async/await` pour les opérations I/O (DB, HTTP)
- Suffixer les méthodes asynchrones avec `Async`
- Ne jamais bloquer avec `.Result` ou `.Wait()`
- Retourner `Task` ou `Task<T>` pour les méthodes async

### Commentaires et Documentation

- **Pas de commentaires évidents** : le code doit être auto-explicatif
- Utiliser des commentaires XML `///` pour les API publiques
- Documenter les algorithmes complexes ou les règles métier
- Éviter les commentaires obsolètes

```csharp
/// <summary>
/// Récupère un Pokémon par son ID MongoDB ou son numéro de Pokédex
/// </summary>
/// <param name="id">ID MongoDB (ObjectId) ou numéro de Pokédex (int)</param>
/// <returns>Le Pokémon trouvé</returns>
/// <exception cref="KeyNotFoundException">Aucun Pokémon trouvé</exception>
public async Task<Pokemon> GetPokemonByIdAsync(string id)
{
    // ...
}
```

### Tests et Validation

- Valider les entrées utilisateur au niveau des DTOs avec `[Required]`, `[Range]`, etc.
- Valider la logique métier dans les Services
- Tester les cas limites (null, vide, valeurs extrêmes)

### Sécurité

- **Authentification JWT** : Toutes les routes sensibles doivent vérifier le token
- **Hashage des mots de passe** : Utiliser BCrypt pour hasher les mots de passe
- **Ne jamais logger les mots de passe** ou tokens
- Valider et échapper les entrées utilisateur
- Utiliser HTTPS en production

---

## 🔧 Technologies et Packages

### Backend

| Projet | Packages Principaux | Version |
|--------|---------------------|---------|
| **PokéDesc.Domain** | MongoDB.Bson | 3.5.0 |
| **PokéDesc.Data** | MongoDB.Driver | 3.5.0 |
| **PokéDesc.Business** | BCrypt.Net-Next, System.IdentityModel.Tokens.Jwt | 4.0.3, 8.14.0 |
| **PokéDesc.API** | Microsoft.AspNetCore.Authentication.JwtBearer, Swashbuckle.AspNetCore | 8.0.11, 6.6.2 |

### Frontend

| Projet | Framework | Version |
|--------|-----------|---------|
| **Projet_FullStack_FrontEnd** | Blazor Server | .NET 8.0 |

### Base de données

- **MongoDB** : Base de données NoSQL document-oriented
- Collections principales : `Pokemon_Collection`, `Dresseurs`, `Parties`

---

## 📝 Workflow de Développement

### Ajout d'une Nouvelle Fonctionnalité

1. **Définir ou modifier les entités** dans `PokéDesc.Domain`
2. **Créer/mettre à jour le Repository** dans `PokéDesc.Data` si besoin
3. **Implémenter la logique métier** dans `PokéDesc.Business` (Service + Interface)
4. **Exposer via l'API** dans `PokéDesc.API` (Controller + DTOs)
5. **Consommer dans le Frontend** (Page Razor + appels HTTP)
6. **Tester** chaque couche indépendamment

### Exemple Concret : Ajouter un système de succès

1. **Domain** : Créer `Succes.cs` avec propriétés
2. **Data** : Créer `SuccesRepository.cs` avec CRUD
3. **Business** : Créer `ISuccesService.cs` et `SuccesService.cs` avec logique de déblocage
4. **API** : Créer `SuccesController.cs` avec endpoints
5. **Frontend** : Créer `Succes.razor` pour afficher les succès

---

## 🚀 Exécution et Configuration

### Backend (API)

**appsettings.json** :
```json
{
  "MongoDbSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "PokeDescDB"
  },
  "Jwt": {
    "Key": "VotreCléSecrèteTrèsLongueEtSécurisée",
    "Issuer": "PokéDescAPI",
    "Audience": "PokéDescApp"
  }
}
```

**Port par défaut** : `http://localhost:5122`

### Frontend

**HttpClient BaseAddress** : Configurée sur `http://localhost:5122/`

**Port par défaut** : Blazor Server utilise généralement `https://localhost:5001`

---

## ✅ Checklist avant de coder

- [ ] Quelle couche est concernée ?
- [ ] Ai-je besoin de créer une interface ?
- [ ] La dépendance respecte-t-elle le flux unidirectionnel ?
- [ ] Les noms suivent-ils les conventions PascalCase ?
- [ ] Les méthodes async sont-elles suffixées avec `Async` ?
- [ ] Les exceptions sont-elles gérées correctement ?
- [ ] Les DTOs sont-ils validés ?
- [ ] Le code est-il simple, lisible et professionnel ?

---

## 📚 Ressources Complémentaires

- **Documentation .NET** : https://learn.microsoft.com/dotnet
- **MongoDB C# Driver** : https://www.mongodb.com/docs/drivers/csharp
- **Blazor** : https://learn.microsoft.com/aspnet/core/blazor
- **Principes SOLID** : https://en.wikipedia.org/wiki/SOLID

---

## 🎓 Philosophie du Code

> "Le code doit être écrit pour être lu par des humains, et accessoirement exécuté par des machines."

- **Simplicité** : Privilégier les solutions simples et directes
- **Clarté** : Noms explicites, structure évidente
- **Cohérence** : Suivre les mêmes patterns partout
- **Maintenabilité** : Penser aux développeurs futurs (vous dans 6 mois)
- **Professionnalisme** : Code propre, testé, documenté

---

**Dernière mise à jour** : Décembre 2025  
**Version du guide** : 1.0
