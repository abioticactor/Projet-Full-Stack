// Dans PokéDesc.Business/Services/DresseurService.cs
using PokéDesc.Data.Repositories;
using PokéDesc.Domain;
using Microsoft.Extensions.Configuration; // Ajout
using Microsoft.IdentityModel.Tokens;     // Ajout
using System.IdentityModel.Tokens.Jwt;  // Ajout
using System.Security.Claims;           // Ajout
using System.Text;                      // Ajout
using BCrypt.Net;                       // Ajout (ou déjà présent)

namespace PokéDesc.Business.Services;

public class DresseurService
{
    private readonly DresseurRepository _dresseurRepository;
    private readonly IConfiguration _config; // Ajout

    // Constructeur mis à jour pour injecter IConfiguration
    public DresseurService(DresseurRepository dresseurRepository, IConfiguration config)
    {
        _dresseurRepository = dresseurRepository;
        _config = config;
    }

    // --- Méthode d'inscription (existante) ---
    public async Task RegisterAsync(string pseudo, string email, string password)
    {
        // Règle métier n°1 : Vérifier si l'email est déjà pris
        var existingDresseurByEmail = await _dresseurRepository.GetByEmailAsync(email);
        if (existingDresseurByEmail != null)
        {
            throw new Exception("Cet email est déjà utilisé.");
        }

        // Règle métier n°2 : Vérifier si le pseudo est déjà pris
        var existingDresseurByPseudo = await _dresseurRepository.GetByPseudoAsync(pseudo);
        if (existingDresseurByPseudo != null)
        {
            throw new Exception("Ce pseudo est déjà utilisé.");
        }

        // Règle métier n°3 : Hacher le mot de passe
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

        var newDresseur = new Dresseur
        {
            Pseudo = pseudo,
            Email = email,
            MotDePasseHash = hashedPassword
        };

        // Demander à la couche Data de sauvegarder
        await _dresseurRepository.CreateAsync(newDresseur);
    }

    // --- NOUVELLE MÉTHODE POUR LE LOGIN ---
    public async Task<string> LoginAsync(string email, string password)
    {
        // 1. Trouver le dresseur
        var dresseur = await _dresseurRepository.GetByEmailAsync(email);

        // 2. Vérifier si il existe ET si le mot de passe est correct
        if (dresseur == null || !BCrypt.Net.BCrypt.Verify(password, dresseur.MotDePasseHash))
        {
            throw new Exception("Email ou mot de passe invalide.");
        }

        // 3. Si tout est OK, générer un token
        return GenerateJwtToken(dresseur!);
    }

    public async Task AjouterAmiAsync(string monId, string pseudoAmi)
    {
        // 1. Trouver l'ami par son pseudo
        var ami = await _dresseurRepository.GetByPseudoAsync(pseudoAmi);
        if (ami == null)
        {
            throw new Exception("Dresseur introuvable.");
        }

        // 2. Trouver mon propre profil
        var moi = await _dresseurRepository.GetByIdAsync(monId);
        if (moi == null)
        {
            throw new Exception("Utilisateur courant introuvable.");
        }

        // 3. Ajouter l'ami (s'il n'y est pas déjà)
        if (!moi.AmisIds.Contains(ami.Id))
        {
            moi.AmisIds.Add(ami.Id);
            await _dresseurRepository.UpdateAsync(moi);
        }
    }

    // --- NOUVELLE MÉTHODE PRIVÉE POUR CRÉER LE TOKEN ---
    private string GenerateJwtToken(Dresseur dresseur)
    {
        // On récupère la clé secrète depuis la configuration (Secret Manager)
        var jwtKey = _config["Jwt:Key"];
        if (string.IsNullOrEmpty(jwtKey))
        {
            throw new InvalidOperationException("La clé secrète JWT (Jwt:Key) n'est pas configurée dans le Secret Manager.");
        }
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        // On définit les "claims" (les infos qu'on met dans le token)
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, dresseur.Id), // L'ID de l'utilisateur
            new Claim(JwtRegisteredClaimNames.Email, dresseur.Email),
            new Claim("pseudo", dresseur.Pseudo) // Claim personnalisé
        };

        // On crée le token
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(24), // Expiration dans 24h
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    public async Task CapturerPokemonAsync(string dresseurId, int pokemonId, int niveau)
    {
        var dresseur = await _dresseurRepository.GetByIdAsync(dresseurId);
        if (dresseur == null) throw new Exception("Dresseur introuvable.");

        // Vérifier si le dresseur a déjà ce Pokémon
        var captureExistante = dresseur.Pokedex.FirstOrDefault(p => p.PokemonId == pokemonId);

        if (captureExistante != null)
        {
            // Si oui, on pourrait imaginer augmenter son niveau ou juste ne rien faire
            // Pour l'instant, on ne fait rien s'il l'a déjà.
            return;
        }

        // Sinon, on crée la nouvelle capture
        var nouvelleCapture = new PokemonCapture
        {
            PokemonId = pokemonId,
            Niveau = niveau
        };

        dresseur.Pokedex.Add(nouvelleCapture);
        
        // On sauvegarde le dresseur mis à jour
        await _dresseurRepository.UpdateAsync(dresseur);
    }

    // Récupérer le Pokédex complet
    public async Task<List<PokemonCapture>> GetPokedexAsync(string dresseurId)
    {
        var dresseur = await _dresseurRepository.GetByIdAsync(dresseurId);
        if (dresseur == null) throw new Exception("Dresseur introuvable.");

        return dresseur.Pokedex;
    }
}