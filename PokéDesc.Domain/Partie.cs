// Dans PokéDesc.Domain/Partie.cs
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Partie
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    // Le code unique pour rejoindre (ex: "AB12CD")
    public string CodeSession { get; set; } = string.Empty;

    // L'ID du créateur de la partie
    public string Dresseur1Id { get; set; } = string.Empty;

    // L'ID du joueur qui rejoint (peut être null au début)
    public string? Dresseur2Id { get; set; }

    // Statut : "EnAttente", "EnCours", "Termine"
    public string Statut { get; set; } = "EnAttente";
    
    // On ajoutera les listes de Pokémon à deviner, etc. plus tard
}