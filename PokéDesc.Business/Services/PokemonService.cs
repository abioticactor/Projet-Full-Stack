using PokéDesc.Data.Repositories;
using PokéDesc.Domain.Models;
using PokéDesc.Business.Interfaces;
using PokéDesc.Business.Models;
using MongoDB.Bson;

namespace PokéDesc.Business.Services;

public class PokemonService : IPokemonService
{
    private readonly PokemonRepository _repository;

    public PokemonService(PokemonRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<Pokemon>> GetAllPokemonsAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<(List<Pokemon> items, int totalCount, int totalPages)> GetPokemonsPaginatedAsync(int page, int pageSize)
    {
        var (items, totalCount) = await _repository.GetPaginatedAsync(page, pageSize);
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        return (items, totalCount, totalPages);
    }

    public async Task<Pokemon> GetPokemonByIdAsync(string id)
    {
        Pokemon? pokemon = null;

        // Vérifie si c'est un ObjectId valide (24 caractères hexadécimaux)
        if (ObjectId.TryParse(id, out _))
        {
            pokemon = await _repository.GetByIdAsync(id);
        }
        // Sinon, essaie de l'interpréter comme un numéro de Pokédex
        else if (int.TryParse(id, out int pokedexNumber))
        {
            pokemon = await _repository.GetByPokedexNumberAsync(pokedexNumber);
        }

        if (pokemon == null)
        {
            throw new KeyNotFoundException($"Pokemon avec l'ID {id} introuvable"    );
        }
        return pokemon;
    }

    public async Task<Pokemon> GetPokemonByPokedexNumberAsync(int pokedexNumber)
    {
        var pokemon = await _repository.GetByPokedexNumberAsync(pokedexNumber);
        if (pokemon == null)
        {
            throw new KeyNotFoundException($"Pokemon #{pokedexNumber} introuvable");
        }
        return pokemon;
    }

    public async Task<List<Pokemon>> GetPokemonsByTypeAsync(string typeName)
    {
        return await _repository.GetByTypeAsync(typeName);
    }

    public async Task<List<Pokemon>> GetPokemonsByGenerationAsync(string generationName)
    {
        return await _repository.GetByGenerationAsync(generationName);
    }

    public async Task<List<Pokemon>> GetLegendaryPokemonsAsync()
    {
        var allPokemons = await _repository.GetAllAsync();
        return allPokemons.Where(p => p.Status.IsLegendary).ToList();
    }

    public async Task<List<Pokemon>> GetMythicalPokemonsAsync()
    {
        var allPokemons = await _repository.GetAllAsync();
        return allPokemons.Where(p => p.Status.IsMythical).ToList();
    }

    public async Task<List<Pokemon>> GetLegendaryOrMythicalPokemonsAsync()
    {
        var allPokemons = await _repository.GetAllAsync();
        return allPokemons.Where(p => p.Status.IsLegendary || p.Status.IsMythical).ToList();
    }

    public async Task<List<Pokemon>> GetBaseEvolutionPokemonsAsync()
    {
        var allPokemons = await _repository.GetAllAsync();
        // Filtrer les Pokémons dont le level est 0 dans leur chaîne d'évolution
        return allPokemons.Where(p => 
        {
            if (p.EvolutionChain?.Chain == null || !p.EvolutionChain.Chain.Any())
                return false;
            
            // Chercher l'entrée correspondant au Pokémon actuel dans la chaîne
            var evolutionEntry = p.EvolutionChain.Chain
                .FirstOrDefault(e => string.Equals(e.Name, p.NameEn, StringComparison.OrdinalIgnoreCase));
            
            // Vérifier si le level est 0
            return evolutionEntry?.Level == 0;
        }).ToList();
    }

    public async Task<string> GetCensoredDescriptionAsync(string id)
    {
        var pokemon = await GetPokemonByIdAsync(id);
        if (string.IsNullOrEmpty(pokemon.Description))
        {
            return string.Empty;
        }
        
        var description = pokemon.Description;
        var nameFr = await GetPokemonNameFrAsync(id);

        // Censure du nom français
        if (!string.IsNullOrWhiteSpace(nameFr))
        {
            description = System.Text.RegularExpressions.Regex.Replace(
                description, 
                System.Text.RegularExpressions.Regex.Escape(nameFr.Trim()), 
                "***", 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }
        return description;
    }

    public async Task<PokemonHints> GetPokemonHintsAsync(string id)
    {
        var pokemon = await GetPokemonByIdAsync(id);
        
        return new PokemonHints
        {
            Category = pokemon.Category,
            Generation = pokemon.Generation,
            Region = pokemon.Region,
            Types = pokemon.Types,
            Status = pokemon.Status,
            Breeding = pokemon.Breeding,
            Physical = pokemon.Physical,
            Abilities = pokemon.Abilities,
            Stats = pokemon.Stats,
            Sprites = pokemon.Sprites,
            Cries = pokemon.Cries,
            EvolutionChainCount = pokemon.EvolutionChain?.Count ?? 1
        };
    }

    public async Task<Pokemon> CreatePokemonAsync(Pokemon pokemon)
    {
        // Validation métier ici si nécessaire
        if (string.IsNullOrWhiteSpace(pokemon.NameFr))
        {
            throw new ArgumentException("Le nom français est requis");
        }

        return await _repository.CreateAsync(pokemon);
    }

    public async Task<Pokemon> UpdatePokemonAsync(string id, Pokemon pokemon)
    {
        var success = await _repository.UpdateAsync(id, pokemon);
        if (!success)
        {
            throw new KeyNotFoundException($"Pokemon avec l'ID {id} introuvable");
        }
        return pokemon;
    }

    public async Task<bool> DeletePokemonAsync(string id)
    {
        var success = await _repository.DeleteAsync(id);
        if (!success)
        {
            throw new KeyNotFoundException($"Pokemon avec l'ID {id} introuvable");
        }
        return true;
    }

    public async Task<string> GetPokemonNameFrAsync(string id)
    {
        var pokemon = await GetPokemonByIdAsync(id);
        return pokemon.NameFr;
    }
}
