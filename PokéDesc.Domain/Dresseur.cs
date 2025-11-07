// Dans PokéDesc.Domain/Dresseur.cs
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Dresseur
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    public string Pseudo { get; set; }
    public string Email { get; set; }
    public string MotDePasseHash { get; set; }
    // On ajoutera les autres propriétés (niveau, pokemons...) plus tard
}