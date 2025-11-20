// Dans PokéDesc.Domain/Dresseur.cs
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace PokéDesc.Domain;

public class Dresseur
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!; // On dit à C# que MongoDB va s'en occuper

    public string Pseudo { get; set; } = string.Empty; // On initialise à une chaîne vide
    public string Email { get; set; } = string.Empty;
    public string MotDePasseHash { get; set; } = string.Empty;

    // --- AJOUT ---
    // On va stocker les ID des dresseurs qui sont amis
    public List<string> AmisIds { get; set; } = new List<string>();

    // On stocke aussi les Pokémon capturés par le dresseur
    public List<PokemonCapture> Pokedex { get; set; } = new List<PokemonCapture>();
}