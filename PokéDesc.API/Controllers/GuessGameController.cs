using Microsoft.AspNetCore.Mvc;
using PokéDesc.Business.Interfaces;
using PokéDesc.API.DTOs;

namespace PokéDesc.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GuessGameController : ControllerBase
{
    private readonly IGuessGameService _gameService;

    public GuessGameController(IGuessGameService gameService)
    {
        _gameService = gameService;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateGame([FromBody] CreateGameRequest request)
    {
        var partie = await _gameService.CreateGameAsync(request.DresseurId);
        return Ok(partie);
    }

    [HttpPost("join")]
    public async Task<IActionResult> JoinGame([FromBody] JoinGameRequest request)
    {
        try
        {
            var partie = await _gameService.JoinGameAsync(request.CodeSession, request.DresseurId);
            return Ok(partie);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("{partieId}")]
    public async Task<IActionResult> GetGame(string partieId)
    {
        try
        {
            var partie = await _gameService.GetGameAsync(partieId);
            return Ok(partie);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("{partieId}/guess")]
    public async Task<IActionResult> SubmitGuess(string partieId, [FromBody] SubmitGuessRequest request)
    {
        try
        {
            var result = await _gameService.SubmitGuessAsync(partieId, request.DresseurId, request.PokemonName);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("{partieId}/hint")]
    public async Task<IActionResult> UseHint(string partieId, [FromBody] UseHintRequest request)
    {
        try
        {
            var partie = await _gameService.UseHintAsync(partieId, request.DresseurId, request.HintType);
            return Ok(partie);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
