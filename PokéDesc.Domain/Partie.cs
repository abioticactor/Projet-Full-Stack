#nullable enable
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using PokéDesc.Domain.Models;

namespace PokéDesc.Domain;

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

    // --- Logique du Jeu ---

    // La liste des 6 Pokémon à deviner pour le Joueur 1
    public List<Pokemon> PokemonsToGuessJ1 { get; set; } = new List<Pokemon>();

    // La liste des 6 Pokémon à deviner pour le Joueur 2
    public List<Pokemon> PokemonsToGuessJ2 { get; set; } = new List<Pokemon>();

    // Progression Joueur 1
    public int CurrentIndexJ1 { get; set; } = 0;
    public int ScoreJ1 { get; set; } = 0;
    
    // État du tour actuel (Joueur 1)
    public int AttemptsUsedJ1 { get; set; } = 0;
    public List<string> UsedHintsJ1 { get; set; } = new List<string>();

    // Progression Joueur 2
    public int CurrentIndexJ2 { get; set; } = 0;
    public int ScoreJ2 { get; set; } = 0;
    
    // État du tour actuel (Joueur 2)
    public int AttemptsUsedJ2 { get; set; } = 0;
    public List<string> UsedHintsJ2 { get; set; } = new List<string>();
}
