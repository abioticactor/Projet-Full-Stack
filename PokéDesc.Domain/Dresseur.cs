// Dans PokéDesc.Domain/Dresseur.cs
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Dresseur
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!; // On dit à C# que MongoDB va s'en occuper

    public string Pseudo { get; set; } = string.Empty; // On initialise à une chaîne vide
    public string Email { get; set; } = string.Empty;
    public string MotDePasseHash { get; set; } = string.Empty;
    // On ajoutera les autres propriétés (niveau, pokemons...) plus tard
}