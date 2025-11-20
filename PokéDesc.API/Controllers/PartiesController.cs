// Dans PokéDesc.API/Controllers/PartiesController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PokéDesc.Business.Services;
using System.Security.Claims;

namespace PokéDesc.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // 👈 TOUT ce contrôleur est protégé
public class PartiesController : ControllerBase
{
    private readonly PartieService _partieService;

    public PartiesController(PartieService partieService)
    {
        _partieService = partieService;
    }

    // Endpoint pour créer une nouvelle partie
    [HttpPost("creer")]
    public async Task<IActionResult> CreerPartie()
    {
        var monId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(monId)) return Unauthorized();

        var partie = await _partieService.CreerSessionAsync(monId);
        // On renvoie le code que l'autre joueur devra utiliser
        return Ok(new { code = partie.CodeSession });
    }

    // Endpoint pour rejoindre une partie
    [HttpPost("rejoindre")]
    public async Task<IActionResult> RejoindrePartie([FromBody] RejoindreRequest request)
    {
        var monId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(monId)) return Unauthorized();

        try
        {
            var partie = await _partieService.RejoindreSessionAsync(monId, request.CodeSession.ToUpper());
            // On renvoie l'objet "Partie" complet
            return Ok(partie);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

public record RejoindreRequest(string CodeSession);