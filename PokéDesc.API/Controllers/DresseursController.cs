// Dans PokéDesc.API/Controllers/DresseursController.cs
using Microsoft.AspNetCore.Mvc;
using PokéDesc.Business.Services;

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

    // --- NOUVEL ENDPOINT POUR LE LOGIN ---
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
}

// Record pour la requête d'inscription (existant)
public record RegisterRequest(string Pseudo, string Email, string Password);

// NOUVEAU Record pour la requête de login
public record LoginRequest(string Email, string Password);