using PokéDesc.Business.Interfaces;
using PokéDesc.Business.Models;
using PokéDesc.Domain;
using PokéDesc.Domain.Models;

namespace PokéDesc.Business.Services;

public class PartieService : IPartieService
{
    private readonly IPokemonService _pokemonService;
    // TODO: Injecter un Repository pour sauvegarder la Partie (ex: IPartieRepository)
    // Pour l'instant, on va simuler le stockage en mémoire ou supposer qu'il existe.
    private static readonly List<Partie> _fakeGameStore = new(); 

    // Configuration des coûts des indices
    private static readonly Dictionary<string, int> HintCosts = new()
    {
        { "Type", 20 },
        { "Generation", 15 },
        { "Category", 10 },
        { "Stats", 15 },
        { "Physical", 10 },
        { "Abilities", 25 }
    };

    private const int MaxAttempts = 3;
    private const int BaseScore = 100;
    private const int PokemonsPerGame = 6;

    public PartieService(IPokemonService pokemonService)
    {
        _pokemonService = pokemonService;
    }

    public async Task<Partie> CreateGameAsync(string dresseurId)
    {
        var partie = new Partie
        {
            Id = Guid.NewGuid().ToString(), // Simulé, MongoDB le ferait auto
            CodeSession = GenerateSessionCode(),
            Dresseur1Id = dresseurId,
            PokemonsToGuessJ1 = new List<Pokemon>(),
            PokemonsToGuessJ2 = new List<Pokemon>(),
            Statut = "EnAttente" 
        };

        _fakeGameStore.Add(partie);
        return await Task.FromResult(partie);
    }

    public async Task<Partie> StartGameAsync(string partieId, string mode)
    {
        var partie = await GetGameAsync(partieId);

        if (mode == "Standard")
        {
            int numberOfPokemons = 6;
            // Génération de 6 Pokémons uniques pour chaque joueur
            partie.PokemonsToGuessJ1 = await SelectRandomPokemonsAsync(numberOfPokemons);
            partie.PokemonsToGuessJ2 = await SelectRandomPokemonsAsync(numberOfPokemons);
            partie.Statut = "EnCours";
        }
        else
        {
            throw new ArgumentException($"Mode de jeu '{mode}' inconnu.");
        }

        return partie;
    }

    private async Task<List<Pokemon>> SelectRandomPokemonsAsync(int count)
    {
        if (count < 1 || count > 6)
        {
            throw new ArgumentException("Le nombre de Pokémon doit être compris entre 1 et 6.");
        }

        var allPokemons = await _pokemonService.GetAllPokemonsAsync();
        var random = new Random();

        return allPokemons.OrderBy(x => random.Next()).Take(count).ToList();
    }

    public async Task<Partie> JoinGameAsync(string codeSession, string dresseurId)
    {
        var partie = _fakeGameStore.FirstOrDefault(p => p.CodeSession == codeSession);
        if (partie == null) throw new KeyNotFoundException("Partie introuvable.");
        
        partie.Dresseur2Id = dresseurId;
        partie.Statut = "EnCours";
        
        return await Task.FromResult(partie);
    }

    public async Task<Partie> GetGameAsync(string partieId)
    {
        var partie = _fakeGameStore.FirstOrDefault(p => p.Id == partieId);
        if (partie == null) throw new KeyNotFoundException("Partie introuvable.");
        return await Task.FromResult(partie);
    }

    public async Task<Partie> UseHintAsync(string partieId, string dresseurId, string hintType)
    {
        var partie = await GetGameAsync(partieId);
        
        // Vérifier si c'est le tour du joueur (simplifié ici pour J1)
        bool isJ1 = dresseurId == partie.Dresseur1Id;
        var usedHints = isJ1 ? partie.UsedHintsJ1 : partie.UsedHintsJ2;

        if (!HintCosts.ContainsKey(hintType))
        {
            throw new ArgumentException($"Type d'indice '{hintType}' inconnu.");
        }

        if (!usedHints.Contains(hintType))
        {
            usedHints.Add(hintType);
        }

        return partie;
    }

    public async Task<GuessResult> SubmitGuessAsync(string partieId, string dresseurId, string pokemonName)
    {
        var partie = await GetGameAsync(partieId);
        bool isJ1 = dresseurId == partie.Dresseur1Id;
        
        int currentIndex = isJ1 ? partie.CurrentIndexJ1 : partie.CurrentIndexJ2;
        var pokemonsList = isJ1 ? partie.PokemonsToGuessJ1 : partie.PokemonsToGuessJ2;
        
        // Vérifier si la partie est finie pour ce joueur
        if (currentIndex >= pokemonsList.Count)
        {
            return new GuessResult { IsGameFinished = true, Message = "Partie déjà terminée.", UpdatedGame = partie };
        }

        var targetPokemon = pokemonsList[currentIndex];
        
        // Normalisation pour la comparaison (minuscule, trim)
        bool isCorrect = string.Equals(targetPokemon.NameFr, pokemonName, StringComparison.OrdinalIgnoreCase);

        if (isCorrect)
        {
            // Calcul du score
            int points = CalculateScore(isJ1 ? partie.UsedHintsJ1 : partie.UsedHintsJ2);
            
            if (isJ1) partie.ScoreJ1 += points;
            else partie.ScoreJ2 += points;

            AdvanceToNextPokemon(partie, isJ1);

            return new GuessResult
            {
                IsCorrect = true,
                IsTurnFinished = true,
                PointsEarned = points,
                Message = $"Bravo ! C'était bien {targetPokemon.NameFr}.",
                UpdatedGame = partie,
                IsGameFinished = CheckIfGameFinished(partie, isJ1)
            };
        }
        else
        {
            // Mauvaise réponse
            if (isJ1) partie.AttemptsUsedJ1++;
            else partie.AttemptsUsedJ2++;

            int attemptsUsed = isJ1 ? partie.AttemptsUsedJ1 : partie.AttemptsUsedJ2;

            if (attemptsUsed >= MaxAttempts)
            {
                // Perdu pour ce Pokémon
                AdvanceToNextPokemon(partie, isJ1);
                
                return new GuessResult
                {
                    IsCorrect = false,
                    IsTurnFinished = true,
                    PointsEarned = 0,
                    Message = $"Dommage, c'était {targetPokemon.NameFr}. Vous passez au suivant.",
                    UpdatedGame = partie,
                    IsGameFinished = CheckIfGameFinished(partie, isJ1)
                };
            }
            else
            {
                // Encore des essais
                return new GuessResult
                {
                    IsCorrect = false,
                    IsTurnFinished = false,
                    PointsEarned = 0,
                    Message = $"Ce n'est pas ça. Il vous reste {MaxAttempts - attemptsUsed} essais.",
                    UpdatedGame = partie
                };
            }
        }
    }

    private void AdvanceToNextPokemon(Partie partie, bool isJ1)
    {
        if (isJ1)
        {
            partie.CurrentIndexJ1++;
            partie.AttemptsUsedJ1 = 0;
            partie.UsedHintsJ1.Clear();
        }
        else
        {
            partie.CurrentIndexJ2++;
            partie.AttemptsUsedJ2 = 0;
            partie.UsedHintsJ2.Clear();
        }
    }

    private int CalculateScore(List<string> usedHints)
    {
        int score = BaseScore;
        foreach (var hint in usedHints)
        {
            if (HintCosts.TryGetValue(hint, out int cost))
            {
                score -= cost;
            }
        }
        return Math.Max(0, score); // Pas de score négatif
    }

    private bool CheckIfGameFinished(Partie partie, bool isJ1)
    {
        int index = isJ1 ? partie.CurrentIndexJ1 : partie.CurrentIndexJ2;
        var pokemonsList = isJ1 ? partie.PokemonsToGuessJ1 : partie.PokemonsToGuessJ2;
        return index >= pokemonsList.Count;
    }

    private string GenerateSessionCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 6).Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
