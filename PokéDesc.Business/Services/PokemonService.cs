using PokéDesc.Data.Repositories;
using PokéDesc.Domain.Models;

namespace PokéDesc.Business.Services;

public class PokemonService
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

    public async Task<Pokemon> GetPokemonByIdAsync(string id)
    {
        var pokemon = await _repository.GetByIdAsync(id);
        if (pokemon == null)
        {
            throw new KeyNotFoundException($"Pokemon avec l'ID {id} introuvable");
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
}
