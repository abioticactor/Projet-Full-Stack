using PokéDesc.Business.Interfaces;
using PokéDesc.Business.Models;
using PokéDesc.Domain;
using PokéDesc.Domain.Models;

namespace PokéDesc.Business.Services;

public class PartieService : IPartieService
{
    private readonly IPokemonService _pokemonService;
    private readonly DresseurService _dresseurService;
    // TODO: Injecter un Repository pour sauvegarder la Partie (ex: IPartieRepository)
    // Pour l'instant, on va simuler le stockage en mémoire ou supposer qu'il existe.
    private static readonly List<Partie> _fakeGameStore = new();

    // Configuration des coûts des indices
    private static readonly Dictionary<string, int> HintCosts = new()
    {
        { "Type1", 15 },
        { "Type2", 15 },
        { "Generation", 10 },
        { "Category", 10 },
        { "Stats", 20 },
        { "Height", 5 },
        { "Weight", 5 },
        { "Abilities", 25 },
        { "Sprite", 30 }
    };
    
    // Pénalités de temps (en secondes) pour chaque indice
    private static readonly Dictionary<string, double> HintTimePenalties = new()
    {
        { "Type1", 5.0 },
        { "Type2", 5.0 },
        { "Generation", 3.0 },
        { "Category", 3.0 },
        { "Stats", 7.0 },
        { "Height", 2.0 },
        { "Weight", 2.0 },
        { "Abilities", 8.0 },
        { "Sprite", 30.0 }
    };

    private const int MaxAttempts = 3;
    private const int BaseScore = 100;
    private const int PokemonsPerGame = 6;

    public PartieService(IPokemonService pokemonService, DresseurService dresseurService)
    {
        _pokemonService = pokemonService;
        _dresseurService = dresseurService;
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

    public async Task<Partie> StartGameAsync(string partieId, string mode, bool isSolo = false)
    {
        var partie = await GetGameAsync(partieId);

        // Marquer le mode solo dans la partie
        partie.ModeSolo = isSolo;
        partie.GameMode = mode;

        int numberOfPokemons = 6;
        var random = new Random();

        if (mode == "Standard")
        {
            // Mode Standard : Pokémons de base + 1% de chance légendaire/mythique
            var basePokemons = await _pokemonService.GetBaseEvolutionPokemonsAsync();
            var legendaryMythicalPokemons = await _pokemonService.GetLegendaryOrMythicalPokemonsAsync();
            
            // Générer les tirages communs pour déterminer le type de chaque position
            var rarityDraws = new bool[numberOfPokemons];
            for (int i = 0; i < numberOfPokemons; i++)
            {
                // 1% de chance d'avoir un légendaire/mythique
                rarityDraws[i] = random.Next(100) == 0;
            }
            
            // Génération des Pokémons pour chaque joueur selon les tirages
            partie.PokemonsToGuessJ1 = SelectPokemonsBasedOnDraws(rarityDraws, basePokemons, legendaryMythicalPokemons, random);
            partie.PokemonsToGuessJ2 = SelectPokemonsBasedOnDraws(rarityDraws, basePokemons, legendaryMythicalPokemons, random);
        }
        else if (mode == "Etendu")
        {
            // Mode Étendu : Tous les Pokémons (y compris évolutions) + 1% de chance légendaire/mythique
            var allPokemons = await _pokemonService.GetAllPokemonsAsync();
            var legendaryMythicalPokemons = await _pokemonService.GetLegendaryOrMythicalPokemonsAsync();
            
            // Séparer les Pokémons normaux des légendaires/mythiques
            var normalPokemons = allPokemons
                .Where(p => !p.Status.IsLegendary && !p.Status.IsMythical)
                .ToList();
            
            // Générer les tirages avec 1% de chance légendaire
            var rarityDraws = new bool[numberOfPokemons];
            for (int i = 0; i < numberOfPokemons; i++)
            {
                rarityDraws[i] = random.Next(100) == 0;
            }
            
            partie.PokemonsToGuessJ1 = SelectPokemonsBasedOnDraws(rarityDraws, normalPokemons, legendaryMythicalPokemons, random);
            partie.PokemonsToGuessJ2 = SelectPokemonsBasedOnDraws(rarityDraws, normalPokemons, legendaryMythicalPokemons, random);
        }
        else if (mode == "SansLimite")
        {
            // Mode Sans Limite : Pokémons finaux d'évolution + légendaires sans restriction
            var finalEvolutionPokemons = await _pokemonService.GetFinalEvolutionPokemonsAsync();
            
            // Sélectionner aléatoirement parmi tous les Pokémons finaux (légendaires inclus)
            partie.PokemonsToGuessJ1 = SelectRandomPokemons(finalEvolutionPokemons, numberOfPokemons, random);
            partie.PokemonsToGuessJ2 = SelectRandomPokemons(finalEvolutionPokemons, numberOfPokemons, random);
        }
        else
        {
            throw new ArgumentException($"Mode de jeu '{mode}' inconnu. Modes disponibles : Standard, Etendu, SansLimite");
        }

        partie.Statut = "EnCours";
        
        // Initialiser les timers pour chaque joueur
        partie.TimerStartJ1 = DateTime.UtcNow;
        partie.TimeRemainingJ1 = 60.0;
        partie.TimerStartJ2 = DateTime.UtcNow;
        partie.TimeRemainingJ2 = 60.0;

        return partie;
    }

    private List<Pokemon> SelectPokemonsBasedOnDraws(
        bool[] rarityDraws, 
        List<Pokemon> basePokemons, 
        List<Pokemon> legendaryMythicalPokemons,
        Random random)
    {
        var selectedPokemons = new List<Pokemon>();
        
        foreach (var isRare in rarityDraws)
        {
            var sourceList = isRare ? legendaryMythicalPokemons : basePokemons;
            var selectedPokemon = sourceList[random.Next(sourceList.Count)];
            selectedPokemons.Add(selectedPokemon);
        }
        
        return selectedPokemons;
    }

    private List<Pokemon> SelectRandomPokemons(List<Pokemon> pokemonList, int count, Random random)
    {
        var selectedPokemons = new List<Pokemon>();
        
        for (int i = 0; i < count; i++)
        {
            var selectedPokemon = pokemonList[random.Next(pokemonList.Count)];
            selectedPokemons.Add(selectedPokemon);
        }
        
        return selectedPokemons;
    }

    public async Task<Partie> JoinGameAsync(string codeSession, string dresseurId)
    {
        // Normaliser le code de session (supprimer espaces et mettre en majuscules)
        var normalizedCode = codeSession?.Trim().ToUpper();
        
        var partie = _fakeGameStore.FirstOrDefault(p => 
            p.CodeSession?.Trim().ToUpper() == normalizedCode);
            
        if (partie == null) 
            throw new KeyNotFoundException($"Partie introuvable avec le code '{codeSession}'. Codes disponibles: {string.Join(", ", _fakeGameStore.Select(p => p.CodeSession))}");
        
        if (!string.IsNullOrEmpty(partie.Dresseur2Id))
            throw new ArgumentException("Cette partie a déjà deux joueurs.");
        
        partie.Dresseur2Id = dresseurId;
        partie.Statut = "Prêt"; // Les deux joueurs sont connectés, en attente de démarrage
        
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
            
            // Appliquer la pénalité de temps
            if (HintTimePenalties.TryGetValue(hintType, out double timePenalty))
            {
                if (isJ1)
                {
                    partie.TimeRemainingJ1 = Math.Max(0, partie.TimeRemainingJ1 - timePenalty);
                }
                else
                {
                    partie.TimeRemainingJ2 = Math.Max(0, partie.TimeRemainingJ2 - timePenalty);
                }
            }
        }

        return partie;
    }

    public async Task<GuessResult> SubmitGuessAsync(string partieId, string dresseurId, string pokemonName)
    {
        var partie = await GetGameAsync(partieId);
        bool isJ1 = dresseurId == partie.Dresseur1Id;
        
        // Si c'est le marqueur de timeout du frontend, gérer le timeout
        if (pokemonName == "__TIMEOUT__")
        {
            return await HandleTimeout(partie, isJ1);
        }
        
        // Vérifier si le temps est écoulé
        bool isTimedOut = CheckTimeout(partie, isJ1);
        if (isTimedOut)
        {
            return await HandleTimeout(partie, isJ1);
        }
        
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

            // Enregistrer le Pokémon complété
            RecordCompletedPokemon(partie, isJ1, targetPokemon, true, points);

            // Capturer le Pokémon dans le Pokédex du joueur
            await _dresseurService.CapturerPokemonAsync(dresseurId, targetPokemon.PokedexNumber);

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
                RecordCompletedPokemon(partie, isJ1, targetPokemon, false, 0);
                
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

    public void ResetTimer(string partieId, string dresseurId)
    {
        var partie = _fakeGameStore.FirstOrDefault(p => p.Id == partieId);
        if (partie == null)
            return;
            
        bool isJ1 = dresseurId == partie.Dresseur1Id;
        
        if (isJ1)
        {
            partie.TimerStartJ1 = DateTime.UtcNow;
            partie.TimeRemainingJ1 = 60.0;
        }
        else
        {
            partie.TimerStartJ2 = DateTime.UtcNow;
            partie.TimeRemainingJ2 = 60.0;
        }
    }

    private void RecordCompletedPokemon(Partie partie, bool isJ1, Pokemon pokemon, bool wasGuessed, int pointsEarned)
    {
        var completedPokemon = new CompletedPokemon
        {
            PokemonId = pokemon.Id,
            PokemonName = pokemon.NameFr,
            WasGuessed = wasGuessed,
            AttemptsUsed = isJ1 ? partie.AttemptsUsedJ1 : partie.AttemptsUsedJ2,
            HintsUsed = new List<string>(isJ1 ? partie.UsedHintsJ1 : partie.UsedHintsJ2),
            PointsEarned = pointsEarned
        };

        if (isJ1)
            partie.CompletedPokemonsJ1.Add(completedPokemon);
        else
            partie.CompletedPokemonsJ2.Add(completedPokemon);
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

    private bool CheckTimeout(Partie partie, bool isJ1)
    {
        DateTime? timerStart = isJ1 ? partie.TimerStartJ1 : partie.TimerStartJ2;
        double timeRemaining = isJ1 ? partie.TimeRemainingJ1 : partie.TimeRemainingJ2;

        if (timerStart == null)
            return false;

        var elapsed = (DateTime.UtcNow - timerStart.Value).TotalSeconds;
        return elapsed >= timeRemaining;
    }

    private async Task<GuessResult> HandleTimeout(Partie partie, bool isJ1)
    {
        int currentIndex = isJ1 ? partie.CurrentIndexJ1 : partie.CurrentIndexJ2;
        var pokemonsList = isJ1 ? partie.PokemonsToGuessJ1 : partie.PokemonsToGuessJ2;

        if (currentIndex >= pokemonsList.Count)
        {
            return new GuessResult { IsGameFinished = true, Message = "Partie déjà terminée.", UpdatedGame = partie };
        }

        var targetPokemon = pokemonsList[currentIndex];

        // Enregistrer le Pokémon comme raté (timeout)
        RecordCompletedPokemon(partie, isJ1, targetPokemon, false, 0);

        // NE PAS passer au Pokémon suivant automatiquement
        // Le joueur le fera manuellement avec le bouton du popup
        // Mais il faut incrémenter l'index pour que le frontend sache qu'on doit passer au suivant
        AdvanceToNextPokemon(partie, isJ1);

        return new GuessResult
        {
            IsCorrect = false,
            IsTurnFinished = true,
            IsTimeout = true,
            PointsEarned = 0,
            Message = $"Temps écoulé ! C'était {targetPokemon.NameFr}.",
            UpdatedGame = partie,
            IsGameFinished = CheckIfGameFinished(partie, isJ1)
        };
    }

    public double GetRemainingTime(string partieId, string dresseurId)
    {
        var partie = _fakeGameStore.FirstOrDefault(p => p.Id == partieId);
        if (partie == null)
            return 0;

        bool isJ1 = dresseurId == partie.Dresseur1Id;
        DateTime? timerStart = isJ1 ? partie.TimerStartJ1 : partie.TimerStartJ2;
        double timeRemaining = isJ1 ? partie.TimeRemainingJ1 : partie.TimeRemainingJ2;

        if (timerStart == null)
            return timeRemaining;

        var elapsed = (DateTime.UtcNow - timerStart.Value).TotalSeconds;
        return Math.Max(0, timeRemaining - elapsed);
    }
}
