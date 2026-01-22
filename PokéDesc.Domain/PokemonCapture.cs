// Dans PokéDesc.Domain/PokemonCapture.cs
namespace PokéDesc.Domain;

public class PokemonCapture
{
    public int PokemonId { get; set; } // L'ID de référence (ex: 25 pour Pikachu)
    public int Niveau { get; set; }
    public DateTime DateCapture { get; set; } = DateTime.Now;
}