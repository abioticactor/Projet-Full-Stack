using PokéDesc.Domain;
using PokéDesc.Domain.Models;
using PokéDesc.Business.Models;

namespace PokéDesc.Business.Interfaces;

public interface IPartieService
{
    /// <summary>
    /// Crée une nouvelle partie et génère les Pokémon à deviner.
    /// </summary>
    Task<Partie> CreateGameAsync(string dresseurId);

    /// <summary>
    /// Lance la partie avec le mode de jeu choisi.
    /// </summary>
    Task<Partie> StartGameAsync(string partieId, string mode);

    /// <summary>
    /// Permet à un deuxième joueur de rejoindre une partie existante via son code.
    /// </summary>
    Task<Partie> JoinGameAsync(string codeSession, string dresseurId);

    /// <summary>
    /// Soumet une réponse pour le Pokémon en cours.
    /// </summary>
    Task<GuessResult> SubmitGuessAsync(string partieId, string dresseurId, string pokemonName);

    /// <summary>
    /// Utilise un indice pour le Pokémon en cours.
    /// </summary>
    Task<Partie> UseHintAsync(string partieId, string dresseurId, string hintType);

    /// <summary>
    /// Récupère l'état actuel de la partie.
    /// </summary>
    Task<Partie> GetGameAsync(string partieId);
}
