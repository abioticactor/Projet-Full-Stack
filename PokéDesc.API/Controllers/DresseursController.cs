// Dans PokéDesc.API/Controllers/DresseursController.cs
using Microsoft.AspNetCore.Mvc;
using PokéDesc.Business.Services;
using Microsoft.AspNetCore.Authorization; // 👈 AJOUTÉ
using System.Security.Claims;           // 👈 AJOUTÉ

namespace PokéDesc.API.Controllers;

[ApiController]
[Route("api/[controller]")] // URL : /api/dresseurs
public class DresseursController : ControllerBase
{
    private readonly DresseurService _dresseurService;

    public DresseursController(DresseurService dresseurService)
    {
        _dresseurService = dresseurService;
    }

    // --- Endpoint d'inscription (existant) ---
    [HttpPost("register")] // URL : POST /api/dresseurs/register
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            await _dresseurService.RegisterAsync(request.Pseudo, request.Email, request.Password);
            return Ok(new { message = "Inscription réussie !" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // --- Endpoint de login (existant) ---
    [HttpPost("login")] // URL : POST /api/dresseurs/login
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var token = await _dresseurService.LoginAsync(request.Email, request.Password);
            // Si le login réussit, on renvoie le token
            return Ok(new { token = token });
        }
        catch (Exception ex)
        {
            // Si le service renvoie une erreur (ex: mdp invalide), on la renvoie
            return BadRequest(new { message = ex.Message });
        }
    }

    // --- NOUVEL ENDPOINT DE PROFIL (PROTÉGÉ) ---
    [HttpGet("profil")]
    [Authorize] // 👈 C'est le "videur" ! Seuls les utilisateurs connectés y ont accès.
    public IActionResult GetProfil()
    {
        // "User" est un objet spécial qui contient les infos
        // du token de l'utilisateur qui fait l'appel.
        var dresseurId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var pseudo = User.FindFirst("pseudo")?.Value;

        if (dresseurId == null)
        {
            return Unauthorized();
        }

        return Ok(new { id = dresseurId, pseudo = pseudo });
    }

    // --- ENDPOINT POUR RÉCUPÉRER UN DRESSEUR PAR ID ---
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDresseurById(string id)
    {
        try
        {
            var dresseur = await _dresseurService.GetDresseurByIdAsync(id);
            if (dresseur == null)
            {
                return NotFound(new { message = "Dresseur introuvable." });
            }
            return Ok(new { id = dresseur.Id, pseudo = dresseur.Pseudo });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // --- NOUVEL ENDPOINT D'AJOUT D'AMI (PROTÉGÉ) ---
    [HttpPost("amis/ajouter")]
    [Authorize] // 👈 Cet endpoint est aussi protégé
    public async Task<IActionResult> AjouterAmi([FromBody] AjouterAmiRequest request)
    {
        try
        {
            // On récupère l'ID du joueur qui fait l'appel (depuis son token)
            // C'est plus sécurisé que de lui faire confiance
            var monId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(monId))
            {
                return Unauthorized("Token invalide.");
            }

            await _dresseurService.AjouterAmiAsync(monId, request.PseudoAmi);
            return Ok(new { message = "Ami ajouté avec succès." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    // 1. AJOUTER UN POKÉMON (Simule une capture ou une victoire)
    [HttpPost("pokedex/ajouter")]
    [Authorize]
    public async Task<IActionResult> AjouterPokemon([FromBody] AjoutPokemonRequest request)
    {
        var monId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (monId == null) return Unauthorized();

        try
        {
            await _dresseurService.CapturerPokemonAsync(monId, request.PokemonId, request.Niveau);
            return Ok(new { message = "Pokémon ajouté au Pokédex !" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // 2. AFFICHER LE POKÉDEX
    [HttpGet("pokedex")]
    [Authorize]
    public async Task<IActionResult> GetMyPokedex()
    {
        var monId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (monId == null) return Unauthorized();

        try
        {
            var pokedex = await _dresseurService.GetPokedexAsync(monId);
            return Ok(pokedex);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

// Record pour la requête d'inscription (existant)
public record RegisterRequest(string Pseudo, string Email, string Password);

// Record pour la requête de login (existant)
public record LoginRequest(string Email, string Password);

// NOUVEAU Record pour la requête d'ajout d'ami
public record AjouterAmiRequest(string PseudoAmi);

 public record AjoutPokemonRequest(int PokemonId, int Niveau);