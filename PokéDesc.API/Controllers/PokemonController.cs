using Microsoft.AspNetCore.Mvc;
using PokéDesc.Business.Services;
using PokéDesc.Domain.Models;

namespace PokéDesc.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PokemonController : ControllerBase
{
    private readonly PokemonService _service;

    public PokemonController(PokemonService service)
    {
        _service = service;
    }

    /// <summary>
    /// Récupère tous les Pokémon
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var pokemons = await _service.GetAllPokemonsAsync();
            return Ok(pokemons);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur serveur", error = ex.Message });
        }
    }

    /// <summary>
    /// Récupère un Pokémon par son ID MongoDB
    /// </summary>
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

    /// <summary>
    /// Récupère un Pokémon par son numéro de Pokédex
    /// </summary>
    [HttpGet("pokedex/{number}")]
    public async Task<IActionResult> GetByPokedexNumber(int number)
    {
        try
        {
            var pokemon = await _service.GetPokemonByPokedexNumberAsync(number);
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

    /// <summary>
    /// Récupère tous les Pokémon d'un type donné
    /// </summary>
    [HttpGet("type/{typeName}")]
    public async Task<IActionResult> GetByType(string typeName)
    {
        try
        {
            var pokemons = await _service.GetPokemonsByTypeAsync(typeName);
            return Ok(pokemons);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur serveur", error = ex.Message });
        }
    }

    /// <summary>
    /// Récupère tous les Pokémon d'une génération donnée
    /// </summary>
    [HttpGet("generation/{generationName}")]
    public async Task<IActionResult> GetByGeneration(string generationName)
    {
        try
        {
            var pokemons = await _service.GetPokemonsByGenerationAsync(generationName);
            return Ok(pokemons);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur serveur", error = ex.Message });
        }
    }

    /// <summary>
    /// Récupère tous les Pokémon légendaires
    /// </summary>
    [HttpGet("legendary")]
    public async Task<IActionResult> GetLegendary()
    {
        try
        {
            var pokemons = await _service.GetLegendaryPokemonsAsync();
            return Ok(pokemons);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur serveur", error = ex.Message });
        }
    }

    /// <summary>
    /// Récupère tous les Pokémon mythiques
    /// </summary>
    [HttpGet("mythical")]
    public async Task<IActionResult> GetMythical()
    {
        try
        {
            var pokemons = await _service.GetMythicalPokemonsAsync();
            return Ok(pokemons);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur serveur", error = ex.Message });
        }
    }

    /// <summary>
    /// Crée un nouveau Pokémon
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Pokemon pokemon)
    {
        try
        {
            var created = await _service.CreatePokemonAsync(pokemon);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur serveur", error = ex.Message });
        }
    }
}
