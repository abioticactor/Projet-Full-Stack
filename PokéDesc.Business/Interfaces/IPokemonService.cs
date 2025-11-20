using PokéDesc.Domain.Models;

namespace PokéDesc.Business.Interfaces;

public interface IPokemonService
{
    Task<List<Pokemon>> GetAllPokemonsAsync();
    Task<Pokemon> GetPokemonByIdAsync(string id);
    Task<Pokemon> GetPokemonByPokedexNumberAsync(int pokedexNumber);
    Task<List<Pokemon>> GetPokemonsByTypeAsync(string typeName);
    Task<List<Pokemon>> GetPokemonsByGenerationAsync(string generationName);
    Task<List<Pokemon>> GetLegendaryPokemonsAsync();
    Task<List<Pokemon>> GetMythicalPokemonsAsync();
    Task<Pokemon> CreatePokemonAsync(Pokemon pokemon);
    Task<Pokemon> UpdatePokemonAsync(string id, Pokemon pokemon);
    Task<bool> DeletePokemonAsync(string id);
}
