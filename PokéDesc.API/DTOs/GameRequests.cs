using System.ComponentModel.DataAnnotations;

namespace PokéDesc.API.DTOs;

public class CreateGameRequest
{
    [Required]
    public string DresseurId { get; set; } = string.Empty;
}

public class JoinGameRequest
{
    [Required]
    public string CodeSession { get; set; } = string.Empty;
    
    [Required]
    public string DresseurId { get; set; } = string.Empty;
}

public class SubmitGuessRequest
{
    [Required]
    public string DresseurId { get; set; } = string.Empty;
    
    [Required]
    public string PokemonName { get; set; } = string.Empty;
}

public class UseHintRequest
{
    [Required]
    public string DresseurId { get; set; } = string.Empty;
    
    [Required]
    public string HintType { get; set; } = string.Empty; // "Type", "Stats", "Talent"
}
